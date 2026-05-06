# Corevia Transporter - Multi-Transport Abstraction Layer

## 📋 Overview

The Corevia Transporter is a flexible, transport-agnostic abstraction layer that enables seamless communication across multiple transport protocols (HTTP, gRPC, etc.) with a unified interface. It supports dynamic request/response models, per-request transport selection, retry policies, and rich metadata handling.

## 🏗️ Architecture

```
Corevia.Transporter.slnx
├── Abstractions/ (Shared Layer)
│   ├── Core Interfaces
│   │   ├── IRequest, IRequest<TResponse> - Enhanced request contracts
│   │   ├── IResponse, IResponse<TData> - Enhanced response contracts
│   │   ├── IDynamicObject - Dynamic property support
│   │   ├── ITransportClient - Transport abstraction
│   │   ├── ITransportClientFactory - Factory pattern
│   │   └── ITransportSelector - Per-request selection
│   │
│   ├── Dynamic Models
│   │   ├── DynamicRequest, DynamicRequest<T> - Base dynamic request
│   │   ├── HttpRequest, HttpRequest<T> - HTTP-specific request
│   │   ├── GrpcRequest, GrpcRequest<T> - gRPC-specific request
│   │   ├── DynamicResponse, DynamicResponse<T> - Base dynamic response
│   │   ├── HttpResponse, HttpResponse<T> - HTTP-specific response
│   │   └── GrpcResponse, GrpcResponse<T> - gRPC-specific response
│   │
│   ├── Metadata Contracts
│   │   ├── IRequestMetadata - Request metadata interface
│   │   ├── IResponseMetadata - Response metadata interface
│   │   ├── ResponseError - Error information
│   │   ├── ValidationError - Validation failure details
│   │   ├── RetryPolicy - Retry configuration
│   │   └── ErrorSeverity - Error severity levels
│   │
│   ├── Implementations
│   │   ├── TransportClientFactory - Default factory
│   │   └── DefaultTransportSelector - Smart selector with route hints
│   │
│   └── Utilities
│       ├── RequestResponseConversion - Type conversion helpers
│       ├── TransporterExamples - Usage examples
│       └── ResponseWrapperProviderType - Response wrapper types
│
├── Corevia.Transporter.Http/ (HTTP Implementation)
│   ├── HttpTransportClient - ITransportClient implementation
│   ├── HttpRequest, HttpRequest<T> - HTTP request types
│   ├── HttpResponse, HttpResponse<T> - HTTP response types
│   ├── IHttpClientService - Legacy service interface
│   ├── Builders/ - Request building utilities
│   ├── Handlers/ - Response handling utilities
│   └── HttpInterceptor/ - HTTP interception/middleware
│
└── Corevia.Transporter.GRPC/ (gRPC Implementation)
    ├── GrpcTransportClient - ITransportClient implementation
    ├── GrpcRequest, GrpcRequest<T> - gRPC request types
    ├── GrpcResponse, GrpcResponse<T> - gRPC response types
    ├── GrpcClientServiceOptions - gRPC configuration
    ├── Builders/ - Protocol buffer building
    └── Handlers/ - gRPC response handling
```

## 🚀 Key Features

### 1. **Dynamic Property Support**
All request/response objects support flexible property handling:

```csharp
var request = new DynamicRequest("/api/users");
request.SetProperty("UserId", 123);
request.SetProperty("IncludeDetails", true);

var userId = request.GetProperty("UserId");
var allProps = request.GetAllProperties();
```

### 2. **Rich Metadata Management**
Comprehensive request and response metadata:

```csharp
// Request metadata
request.Metadata.Headers["Authorization"] = "Bearer token";
request.Metadata.CorrelationId = Guid.NewGuid().ToString();
request.Metadata.Timeout = TimeSpan.FromSeconds(30);
request.Metadata.ContentType = "application/json";
request.Metadata.Attributes["TenantId"] = "tenant-123";

// Response metadata
response.Metadata.StatusCode // HTTP or gRPC status
response.Metadata.Errors // List of errors
response.Metadata.ValidationErrors // Field validation errors
response.Metadata.ElapsedTime // Request duration
response.Metadata.IsCached // Whether response was cached
response.Metadata.ReceivedAt // When response arrived
```

### 3. **Per-Request Transport Selection**
Choose transport at runtime based on business logic:

```csharp
var context = new TransportSelectionContext
{
    UserId = "user-123",
    PreferredTransport = "Grpc", // HTTP or Grpc
    Attributes = 
    {
        ["region"] = "us-west",
        ["useNewApi"] = true
    }
};

var transport = selector.SelectTransport(request, context);
```

### 4. **Retry Policy Configuration**
Built-in retry handling with exponential backoff:

```csharp
var retryPolicy = new RetryPolicy
{
    MaxRetries = 3,
    InitialDelay = TimeSpan.FromMilliseconds(100),
    MaxDelay = TimeSpan.FromSeconds(10),
    BackoffMultiplier = 2.0,
    RetryableStatusCodes = new() { 408, 429, 500, 502, 503, 504 }
};

var options = new TransportOptions { RetryPolicy = retryPolicy };
```

### 5. **Multiple Simultaneous Connections**
Support for named configurations:

```csharp
var options = new TransportOptions
{
    ConfigurationName = "production", // Named connection
    Timeout = TimeSpan.FromSeconds(60)
};

var response = await client.SendAsync<MyResponse>(request, options);
```

### 6. **Type-Safe Request/Response**
Strongly-typed generic support:

```csharp
public class GetUserRequest : HttpRequest<GetUserResponse>
{
    public int UserId { get; set; }
    
    public GetUserRequest(int userId)
    {
        Route = $"/api/users/{userId}";
        HttpMethod = HttpMethod.Get;
        UserId = userId;
    }
}

var response = await client.SendAsync<GetUserResponse>(new GetUserRequest(123));
```

## 📖 Usage Examples

### Example 1: Basic HTTP Request

```csharp
// Create request
var request = new HttpRequest("/api/users/123", HttpMethod.Get);
request.Metadata.Headers["Authorization"] = "Bearer token";

// Send
var response = await httpClient.SendAsync<UserResponse>(request);

if (response.IsSuccess)
{
    Console.WriteLine($"User: {response.Data?.Name}");
}
```

### Example 2: gRPC Request

```csharp
// Create request
var request = new GrpcRequest<ProductResponse>("ProductService", "GetProduct");
request.SetProperty("ProductId", "PROD-001");

// Send
var response = await grpcClient.SendAsync<ProductResponse>(request);
```

### Example 3: Transport Selection with Feature Flags

```csharp
var request = new DynamicRequest<OrderResponse>("/api/orders");

// Route-based: /grpc/ routes use gRPC
if (request.Metadata.Route.Contains("/grpc/"))
{
    var context = new TransportSelectionContext
    {
        PreferredTransport = "Grpc"
    };
    var transport = selector.SelectTransport(request, context);
}
```

### Example 4: Error Handling

```csharp
var response = await client.SendAsync<MyResponse>(request);

if (!response.IsSuccess)
{
    // Check for errors
    foreach (var error in response.Metadata.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Message}");
    }
    
    // Check for validation errors
    foreach (var validation in response.Metadata.ValidationErrors)
    {
        Console.WriteLine($"{validation.PropertyName}: {validation.Message}");
    }
}
```

### Example 5: Dependency Injection Setup

```csharp
// Register transports
services.AddScoped<IHttpClientFactory, HttpClientFactory>();
services.AddScoped<IHttpClientService, HttpClientService>();
services.AddScoped(sp => new HttpTransportClient(sp.GetRequiredService<IHttpClientService>()));
services.AddScoped(sp => new GrpcTransportClient(new GrpcClientServiceOptions()));

// Register factory
services.AddScoped<ITransportClientFactory>(sp =>
{
    var factory = new TransportClientFactory();
    factory.RegisterClient("Http", sp.GetRequiredService<HttpTransportClient>());
    factory.RegisterClient("Grpc", sp.GetRequiredService<GrpcTransportClient>());
    factory.SetDefaultTransport("Http");
    return factory;
});

// Register selector
services.AddScoped<ITransportSelector>(sp =>
{
    var factory = sp.GetRequiredService<ITransportClientFactory>();
    var selector = new DefaultTransportSelector(factory);
    return selector;
});
```

## 🔄 Migration Guide

### From Legacy IRequest to DynamicRequest

```csharp
// Old
var request = new BaseRequest();

// New - Option 1: Use DynamicRequest
var request = new DynamicRequest("/api/endpoint");

// New - Option 2: Use specific transport type
var request = new HttpRequest("/api/endpoint", HttpMethod.Post);
var request = new GrpcRequest("ServiceName", "MethodName");

// New - Option 3: Convert existing
var dynamicRequest = legacyRequest.ConvertToDynamic();
```

### From Legacy IResponse to DynamicResponse

```csharp
// Old
var response = new BaseResponse();

// New
var response = new DynamicResponse();
var response = new DynamicResponse<UserData>();
var response = legacyResponse.ConvertToDynamic();
```

## 📊 Response Status Codes

### HTTP Status Codes
- 200-299: Success
- 400: Bad Request / Validation Error
- 401: Unauthorized
- 403: Forbidden
- 404: Not Found
- 408: Request Timeout (retryable)
- 429: Too Many Requests (retryable)
- 500-599: Server Errors (some retryable)

### gRPC Status Codes
- 0: OK
- 1: CANCELLED (retryable)
- 2: UNKNOWN
- 3: INVALID_ARGUMENT
- 4: DEADLINE_EXCEEDED (retryable)
- 8: RESOURCE_EXHAUSTED (retryable)
- 13: INTERNAL (retryable)
- 14: UNAVAILABLE (retryable)

## 🔒 Security Considerations

1. **Correlation ID**: Use for tracing and audit logging
2. **Headers**: Securely pass tokens and credentials
3. **Timeout**: Prevent hanging requests
4. **Retry Policy**: Configure safe retry behavior
5. **Validation**: Validate all dynamic properties before use

## 📝 File Structure

```
Abstractions/
├── IRequest.cs (original, simplified)
├── IResponse.cs (original, simplified)
├── IRequestEnhanced.cs (new enhanced interfaces)
├── IResponseEnhanced.cs (new enhanced interfaces)
├── DynamicRequest.cs (request implementations)
├── DynamicResponse.cs (response implementations)
├── ITransportClient.cs (transport interface)
├── ITransportClientFactory.cs (factory/selector)
├── ResponseWrapperProviderType.cs (wrapper types)
├── TransportClientFactory.cs (default implementations)
├── RequestResponseConversion.cs (conversion utilities)
└── TransporterExamples.cs (usage examples)
```

## 🧪 Testing

Test both transport types:
```csharp
[Theory]
[InlineData("Http")]
[InlineData("Grpc")]
public async Task TestTransportSending(string transportName)
{
    var transport = factory.GetClient(transportName);
    var response = await transport.SendAsync<MyResponse>(request);
    Assert.True(response.IsSuccess);
}
```

## 🤝 Contributing

To add a new transport:
1. Create a new `Corevia.Transporter.{Protocol}` project
2. Implement `ITransportClient`
3. Create `{Protocol}Request` and `{Protocol}Response` classes (optional)
4. Register in factory
5. Add route/context-based selection logic if needed

## 📄 License

Corevia Transporter - All Rights Reserved

## 📞 Support

For questions or issues, refer to the TransporterExamples.cs file for comprehensive usage patterns.
