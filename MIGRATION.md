# Corevia Transporter - Migration Guide

## Migrating from v1.0 to v2.0

This guide helps you migrate your existing code to use the new Corevia Transporter v2.0 architecture.

## Overview of Changes

| Aspect | v1.0 | v2.0 | Migration Effort |
|--------|------|------|------------------|
| Request Type | BaseRequest | DynamicRequest, HttpRequest | Low |
| Response Type | BaseResponse | DynamicResponse, HttpResponse | Low |
| Transport | IHttpClientService only | ITransportClient (HTTP/gRPC) | Medium |
| Metadata | Limited | Rich (errors, timing, validation) | Low |
| Properties | Type-safe only | Type-safe + dynamic | Low |
| Transport Selection | N/A | Runtime per-request | Medium |
| Error Handling | Basic | Structured with severity | Low |

## Step-by-Step Migration

### Phase 1: Update Namespaces (Required)

**Before:**
```csharp
using HashtApp.Soft.Client.Utilities;

public class UserRequest : BaseRequest { }
public class UserResponse : BaseResponse { }
```

**After:**
```csharp
using Corevia.Transporter.Abstractions;
using Corevia.Transporter.Http;

public class UserRequest : HttpRequest<UserResponse> { }
public class UserResponse : HttpResponse<User> { }
```

### Phase 2: Update Request Classes (Optional but Recommended)

#### Old Pattern - Inheriting from BaseRequest

```csharp
// v1.0 - Old
public class GetUserRequest : BaseRequest
{
    public int UserId { get; set; }
    public HttpMethod HttpMethod => HttpMethod.Get;
    public string Route => $"/api/users/{UserId}";
}
```

#### New Pattern 1 - Using HttpRequest (Recommended)

```csharp
// v2.0 - New (Recommended)
public class GetUserRequest : HttpRequest<GetUserResponse>
{
    public int UserId { get; set; }

    public GetUserRequest(int userId)
    {
        UserId = userId;
        Route = $"/api/users/{userId}";
        HttpMethod = System.Net.Http.HttpMethod.Get;
    }
}

public class GetUserResponse : HttpResponse<User>
{
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
```

#### New Pattern 2 - Using DynamicRequest (Most Flexible)

```csharp
// v2.0 - Alternative with dynamic properties
var request = new DynamicRequest($"/api/users/{userId}", "GET");
request.SetProperty("UserId", userId);
request.Metadata.Headers["Authorization"] = "Bearer token";

var response = await client.SendAsync(request, typeof(GetUserResponse));
```

### Phase 3: Update Service Calls

#### Old Pattern - Using IHttpClientService

```csharp
// v1.0 - Old
public class UserService
{
    private readonly IHttpClientService _httpService;

    public UserService(IHttpClientService httpService)
    {
        _httpService = httpService;
    }

    public async Task<User> GetUserAsync(int userId)
    {
        var request = new GetUserRequest { UserId = userId };
        
        var response = await _httpService.SendAsync<UserResponse>(
            request,
            ResponseWrapperProviderType.AcceptedMessage
        );
        
        return response?.Data;
    }
}
```

#### New Pattern - Using ITransportClient

```csharp
// v2.0 - New (recommended)
public class UserService
{
    private readonly ITransportClientFactory _factory;

    public UserService(ITransportClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        var request = new GetUserRequest(userId);
        
        // Get HTTP transport
        var client = _factory.GetClient("Http");
        
        // Send with enhanced metadata
        var response = await client.SendAsync<GetUserResponse>(
            request,
            new TransportOptions 
            { 
                Timeout = TimeSpan.FromSeconds(30),
                CorrelationId = Guid.NewGuid().ToString()
            }
        );
        
        if (response.IsSuccess)
        {
            return response.Data;
        }
        else
        {
            // New rich error handling
            foreach (var error in response.Metadata.Errors)
            {
                _logger.LogError(error.Code, error.Message);
            }
            return null;
        }
    }
}
```

### Phase 4: Add Retry Policies (Optional)

```csharp
// v2.0 - Add retry support
public async Task<User?> GetUserWithRetryAsync(int userId)
{
    var request = new GetUserRequest(userId);
    var client = _factory.GetClient("Http");
    
    var options = new TransportOptions
    {
        RetryPolicy = new RetryPolicy
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(100),
            MaxDelay = TimeSpan.FromSeconds(10),
            BackoffMultiplier = 2.0
        }
    };
    
    var response = await client.SendAsync<GetUserResponse>(request, options);
    return response.Data;
}
```

### Phase 5: Support Multiple Transports (Optional)

```csharp
// v2.0 - Support both HTTP and gRPC
public async Task<User?> GetUserFlexibleAsync(int userId, string? preferredTransport = null)
{
    var request = new DynamicRequest($"/api/users/{userId}");
    
    // Select transport
    var context = new TransportSelectionContext
    {
        PreferredTransport = preferredTransport
    };
    
    var selector = _serviceProvider.GetRequiredService<ITransportSelector>();
    var transportName = selector.SelectTransport(request, context);
    var client = _factory.GetClient(transportName);
    
    var response = await client.SendAsync(
        request,
        typeof(GetUserResponse)
    ) as DynamicResponse<User>;
    
    return response?.Data;
}
```

## Comparison: Old vs New

### Request Handling

**v1.0:**
```csharp
public class CreateOrderRequest : BaseRequest
{
    public CreateOrderRequest(Order order)
    {
        Order = order;
    }
    public Order Order { get; set; }
    public HttpMethod HttpMethod => HttpMethod.Post;
    public string Route => "/api/orders";
}

var response = await _httpService.SendAsync<OrderResponse>(request);
```

**v2.0:**
```csharp
public class CreateOrderRequest : HttpRequest<OrderResponse>
{
    public Order Order { get; set; }
    
    public CreateOrderRequest(Order order)
    {
        Order = order;
        Route = "/api/orders";
        HttpMethod = System.Net.Http.HttpMethod.Post;
        Metadata.ContentType = "application/json";
    }
}

var client = _factory.GetClient("Http");
var response = await client.SendAsync<OrderResponse>(request);
```

### Response Handling

**v1.0:**
```csharp
var response = await _httpService.SendAsync<OrderResponse>(request);

if (response != null)
{
    Console.WriteLine($"Order created: {response.Id}");
}
```

**v2.0:**
```csharp
var response = await client.SendAsync<OrderResponse>(request);

if (response.IsSuccess)
{
    Console.WriteLine($"Order created: {response.Data?.Id}");
    Console.WriteLine($"Correlation ID: {response.Metadata.CorrelationId}");
    Console.WriteLine($"Time taken: {response.Metadata.ElapsedTime}");
}
else
{
    foreach (var error in response.Metadata.Errors)
    {
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
    }
}
```

## Compatibility Mode (Coexistence)

You can run both v1.0 and v2.0 code simultaneously during migration:

```csharp
public class MixedService
{
    private readonly IHttpClientService _oldService;
    private readonly ITransportClientFactory _newFactory;

    // v1.0 code
    public async Task<User> GetUserOldWayAsync(int userId)
    {
        var request = new OldGetUserRequest { UserId = userId };
        var response = await _oldService.SendAsync<OldUserResponse>(request);
        return response?.Data;
    }

    // v2.0 code
    public async Task<User> GetUserNewWayAsync(int userId)
    {
        var request = new NewGetUserRequest(userId);
        var client = _newFactory.GetClient("Http");
        var response = await client.SendAsync<NewUserResponse>(request);
        return response.Data;
    }
}
```

## Migration Checklist

- [ ] Update all namespace imports
- [ ] Update request classes to inherit from HttpRequest/GrpcRequest
- [ ] Update response classes to inherit from HttpResponse/GrpcResponse
- [ ] Update service layer to use ITransportClientFactory
- [ ] Add ITransportClientFactory to DI container
- [ ] Update response handling to use Metadata
- [ ] Add Correlation IDs for tracing
- [ ] Test with both HTTP and optional gRPC
- [ ] Update error handling for structured errors
- [ ] Add retry policies where needed
- [ ] Update tests to use new assertions
- [ ] Remove old using statements (HashtApp.Soft...)
- [ ] Verify relative paths in project files

## Common Issues and Solutions

### Issue 1: "IHttpClientService not found"

**Problem:** Old interface no longer available

**Solution:**
```csharp
// Instead of directly using IHttpClientService
private readonly IHttpClientService _service;

// Use the factory
private readonly ITransportClientFactory _factory;
var client = _factory.GetClient("Http");
```

### Issue 2: "Request doesn't implement IRequest<TResponse>"

**Problem:** Using old BaseRequest with new client

**Solution:**
```csharp
// Create strongly-typed request
public class MyRequest : HttpRequest<MyResponse>
{
    public MyRequest() { Route = "/api/endpoint"; }
}
```

### Issue 3: "Cannot convert old response to new"

**Problem:** Mixing v1.0 and v2.0 response types

**Solution:**
```csharp
// Use conversion helper
var newResponse = oldResponse.ConvertToDynamic();
```

### Issue 4: "Headers not being sent"

**Problem:** Forgot to set headers in metadata

**Solution:**
```csharp
request.Metadata.Headers["Authorization"] = "Bearer token";
// OR
var options = new TransportOptions 
{ 
    Metadata = { ["Authorization"] = "Bearer token" } 
};
```

## Performance Considerations

### Before Migration
- Single transport (HTTP only)
- Limited metadata tracking
- Basic error handling

### After Migration
- Support for multiple transports
- Rich metadata with timing
- Structured error reporting
- Optional request caching
- Retry policies with backoff

**Performance Impact:** Negligible - slightly more memory for metadata, but offset by efficiency gains in error handling and tracing.

## Testing Migration

### Unit Test Migration

**Before:**
```csharp
[Test]
public async Task GetUser_ReturnsUser()
{
    var mockService = new Mock<IHttpClientService>();
    mockService.Setup(x => x.SendAsync<UserResponse>(
        It.IsAny<IRequest>(),
        It.IsAny<ResponseWrapperProviderType>()
    )).ReturnsAsync(new UserResponse { Id = 1 });
    
    var service = new UserService(mockService.Object);
    var user = await service.GetUserAsync(1);
    
    Assert.IsNotNull(user);
}
```

**After:**
```csharp
[Test]
public async Task GetUser_ReturnsUser()
{
    var mockFactory = new Mock<ITransportClientFactory>();
    var mockClient = new Mock<ITransportClient>();
    
    mockFactory.Setup(x => x.GetClient("Http")).Returns(mockClient.Object);
    mockClient.Setup(x => x.SendAsync<UserResponse>(
        It.IsAny<IRequest>(),
        It.IsAny<TransportOptions>()
    )).ReturnsAsync(new UserResponse 
    { 
        Data = new User { Id = 1 },
        Metadata = { StatusCode = 200 }
    });
    
    var service = new UserService(mockFactory.Object);
    var user = await service.GetUserAsync(1);
    
    Assert.IsNotNull(user);
}
```

## Timeline Recommendations

### Week 1
- Update namespaces
- Create new request/response classes
- Update services to use factory

### Week 2
- Add metadata usage (correlation IDs, timeouts)
- Update error handling
- Add retry policies

### Week 3
- Comprehensive testing
- Performance validation
- Documentation updates

### Week 4
- Monitor production
- Optimize based on metrics
- Remove legacy code if applicable

## Rollback Plan

If issues occur:

```csharp
// Inject both old and new services
public class HybridService
{
    private readonly IHttpClientService _oldService;
    private readonly ITransportClientFactory _newFactory;
    private readonly ILogger _logger;

    public async Task<User> GetUserSafeAsync(int userId)
    {
        try
        {
            // Try new way
            var client = _newFactory.GetClient("Http");
            var response = await client.SendAsync<UserResponse>(request);
            return response.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "New transport failed, using legacy");
            
            // Fallback to old way
            var oldResponse = await _oldService.SendAsync<OldUserResponse>(request);
            return oldResponse?.Data;
        }
    }
}
```

## Getting Help

1. Check QUICKSTART.md for common patterns
2. Review TransporterExamples.cs for code samples
3. See ARCHITECTURE.md for design explanations
4. Check CHANGELOG.md for what changed
5. Consult README.md for comprehensive documentation

---

**Migration Complete!** You're now using the modern, flexible Corevia Transporter v2.0 architecture.
