# Corevia Transporter - Quick Start Guide

## 5-Minute Setup

### Step 1: Register Services in DI Container

```csharp
// Program.cs or Startup.cs
using Corevia.Transporter.Abstractions;
using Corevia.Transporter.Http;
using Corevia.Transporter.GRPC;

// Add HTTP client factory (required for HTTP transport)
services.AddHttpClient();

// Register transport implementations
services.AddScoped<IHttpClientService, HttpClientService>();
services.AddScoped(sp => new HttpTransportClient(sp.GetRequiredService<IHttpClientService>()));
services.AddScoped(sp => new GrpcTransportClient(new GrpcClientServiceOptions 
{
    DefaultAddress = "https://grpc-server:50051"
}));

// Register transport factory
services.AddScoped<ITransportClientFactory>(sp =>
{
    var factory = new TransportClientFactory();
    factory.RegisterClient("Http", sp.GetRequiredService<HttpTransportClient>());
    factory.RegisterClient("Grpc", sp.GetRequiredService<GrpcTransportClient>());
    factory.SetDefaultTransport("Http");
    return factory;
});

// Register transport selector
services.AddScoped<ITransportSelector>(sp =>
{
    var factory = sp.GetRequiredService<ITransportClientFactory>();
    return new DefaultTransportSelector(factory);
});
```

### Step 2: Create a Request Class

```csharp
using Corevia.Transporter.Abstractions;

public class GetUserRequest : HttpRequest<GetUserResponse>
{
    public int UserId { get; set; }

    public GetUserRequest(int userId)
    {
        UserId = userId;
        Route = $"/api/v1/users/{userId}";
        HttpMethod = System.Net.Http.HttpMethod.Get;
        Metadata.ContentType = "application/json";
    }
}

public class GetUserResponse : HttpResponse<User>
{
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
```

### Step 3: Send Request

```csharp
public class UserService
{
    private readonly ITransportClientFactory _factory;

    public UserService(ITransportClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        // Create request
        var request = new GetUserRequest(userId);
        
        // Add headers if needed
        request.Metadata.Headers["Authorization"] = "Bearer your-token";

        // Get HTTP transport
        var httpClient = _factory.GetClient("Http");
        
        // Send request
        var response = await httpClient.SendAsync<GetUserResponse>(request);

        // Handle response
        if (response.IsSuccess)
        {
            return response.Data;
        }
        else
        {
            Console.WriteLine($"Error: {response.Metadata.Message}");
            return null;
        }
    }
}
```

## Common Patterns

### Pattern 1: Switch Between HTTP and gRPC

```csharp
public async Task<T> FetchDataAsync<T>(
    IRequest request,
    string preferredTransport = "Http")
    where T : class, IResponse
{
    var context = new TransportSelectionContext
    {
        PreferredTransport = preferredTransport
    };

    var selector = _factory.GetServices<ITransportSelector>().First();
    var transportName = selector.SelectTransport(request, context);
    var transport = _factory.GetClient(transportName);

    return await transport.SendAsync<T>(request);
}
```

### Pattern 2: Add Retry Policy

```csharp
public async Task<T> SendWithRetryAsync<T>(
    IRequest request,
    int maxRetries = 3)
    where T : class, IResponse
{
    var options = new TransportOptions
    {
        RetryPolicy = new RetryPolicy
        {
            MaxRetries = maxRetries,
            InitialDelay = TimeSpan.FromMilliseconds(100),
            BackoffMultiplier = 2.0
        }
    };

    var client = _factory.GetDefaultClient();
    return await client.SendAsync<T>(request, options);
}
```

### Pattern 3: Add Correlation ID for Tracing

```csharp
public async Task<T> SendWithTracingAsync<T>(
    IRequest request,
    string? correlationId = null)
    where T : class, IResponse
{
    correlationId ??= Guid.NewGuid().ToString();
    request.Metadata.CorrelationId = correlationId;

    var options = new TransportOptions
    {
        CorrelationId = correlationId
    };

    var client = _factory.GetDefaultClient();
    return await client.SendAsync<T>(request, options);
}
```

### Pattern 4: Dynamic Request with Properties

```csharp
var request = new DynamicRequest("/api/search");
request.SetProperty("Query", "shoes");
request.SetProperty("PageSize", 20);
request.SetProperty("PageNumber", 1);

var response = await client.SendAsync(request, typeof(SearchResponse));
```

### Pattern 5: Error Handling

```csharp
var response = await client.SendAsync<MyResponse>(request);

if (response.IsSuccess)
{
    // Success
    Console.WriteLine($"Data: {response.Data}");
}
else
{
    // Handle business errors
    foreach (var error in response.Metadata.Errors)
    {
        Console.WriteLine($"[{error.Code}] {error.Message}");
    }

    // Handle validation errors
    foreach (var validation in response.Metadata.ValidationErrors)
    {
        Console.WriteLine($"{validation.PropertyName}: {validation.Message}");
    }

    // Log timing if slow
    if (response.Metadata.ElapsedTime?.TotalSeconds > 10)
    {
        Console.WriteLine($"Slow request: {response.Metadata.ElapsedTime}");
    }
}
```

## File Templates

### Request Template

```csharp
public class <YourName>Request : HttpRequest<YourNameResponse>
{
    public string Param1 { get; set; } = "";
    public int Param2 { get; set; }

    public <YourName>Request(string param1, int param2)
    {
        Param1 = param1;
        Param2 = param2;
        Route = "/api/endpoint";
        HttpMethod = System.Net.Http.HttpMethod.Post;
    }
}

public class YourNameResponse : HttpResponse<YourDataType>
{
}

public class YourDataType
{
    // Your response data structure
}
```

### gRPC Request Template

```csharp
public class <YourName>GrpcRequest : GrpcRequest<YourNameResponse>
{
    public string Param1 { get; set; } = "";

    public <YourName>GrpcRequest(string param1)
    {
        Param1 = param1;
        ServiceName = "YourServiceName";
        MethodName = "YourMethodName";
    }
}

public class YourNameResponse : GrpcResponse<YourDataType>
{
}
```

## Troubleshooting

### Issue: "Transport 'Http' is not registered"
**Solution:** Ensure you registered the HttpTransportClient in the factory
```csharp
factory.RegisterClient("Http", httpTransportClient);
```

### Issue: "Cannot find SendAsync method"
**Solution:** Ensure your request implements `IRequest` and response implements `IResponse`

### Issue: Headers not being sent
**Solution:** Set headers in request metadata before sending
```csharp
request.Metadata.Headers["Header-Name"] = "value";
```

### Issue: Correlation ID not in response
**Solution:** Ensure server echoes back the correlation ID in response headers

## Next Steps

1. **Read the README.md** for detailed architecture documentation
2. **Check TransporterExamples.cs** for comprehensive examples
3. **Review ARCHITECTURE.md** for visual flow diagrams
4. **Configure Transport Selection** for your specific use case
5. **Add Custom Request/Response Types** for your domain models

## Common Configuration Scenarios

### Scenario 1: Production (HTTP Only)
```csharp
var factory = new TransportClientFactory();
factory.RegisterClient("Http", httpTransportClient);
factory.SetDefaultTransport("Http");
```

### Scenario 2: Gradual Migration (HTTP with gRPC Option)
```csharp
var factory = new TransportClientFactory();
factory.RegisterClient("Http", httpTransportClient);
factory.RegisterClient("Grpc", grpcTransportClient);
factory.SetDefaultTransport("Http"); // Default to HTTP

// Users in the beta group use gRPC
```

### Scenario 3: Per-Region Configuration
```csharp
var options = new TransportOptions
{
    ConfigurationName = region switch
    {
        "us-east" => "http-us-east",
        "eu-west" => "http-eu-west",
        "ap-southeast" => "grpc-ap-southeast",
        _ => "default"
    }
};
```

### Scenario 4: Feature Flag Based Selection
```csharp
var context = new TransportSelectionContext
{
    Attributes = 
    {
        ["useNewGrpcStack"] = featureFlags.IsEnabled("new-grpc")
    },
    PreferredTransport = featureFlags.IsEnabled("new-grpc") ? "Grpc" : "Http"
};
```

## Performance Tips

1. **Reuse Transport Clients**: Don't create new instances per request
2. **Configure Timeouts**: Set appropriate timeouts for different operations
3. **Use Retry Policies**: Handle transient failures gracefully
4. **Monitor Correlation IDs**: Track slow requests across services
5. **Cache When Possible**: Use `TransportOptions.AllowCached` for read-only data

## Security Tips

1. **Always use HTTPS/TLS**: Set up proper certificates
2. **Secure Token Management**: Don't hardcode tokens
3. **Validate Input**: Use ValidationError reporting
4. **Sanitize Headers**: Don't log sensitive headers
5. **Use CorrelationId**: For audit trails and debugging

## Support

- Check TransporterExamples.cs for more patterns
- Review test files for usage examples
- See ARCHITECTURE.md for detailed flows
