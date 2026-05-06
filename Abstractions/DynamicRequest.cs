namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Dynamic request implementation supporting flexible property handling
/// Base class for all request types (HTTP, gRPC, etc.)
/// </summary>
public class DynamicRequest : IRequest
{
    private readonly Dictionary<string, object?> _properties;
    private readonly RequestMetadata _metadata;

    public Guid Id { get; set; }
    public IRequestMetadata Metadata => _metadata;
    public DateTime CreatedAt { get; set; }

    public DynamicRequest()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        _properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        _metadata = new RequestMetadata();
    }

    public DynamicRequest(string route, string method = "GET")
        : this()
    {
        _metadata.Route = route;
        _metadata.Method = method;
    }

    public object? GetProperty(string name)
    {
        _properties.TryGetValue(name, out var value);
        return value;
    }

    public void SetProperty(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Property name cannot be empty", nameof(name));

        _properties[name] = value;
    }

    public IReadOnlyDictionary<string, object?> GetAllProperties()
    {
        return _properties.AsReadOnly();
    }

    public bool HasProperty(string name)
    {
        return _properties.ContainsKey(name);
    }
}

/// <summary>
/// Strongly-typed dynamic request with response type information
/// </summary>
public class DynamicRequest<TResponse> : DynamicRequest, IRequest<TResponse>
    where TResponse : class, IResponse
{
    public DynamicRequest() : base()
    {
    }

    public DynamicRequest(string route, string method = "GET") : base(route, method)
    {
    }
}

/// <summary>
/// HTTP-specific request with HTTP-specific properties
/// </summary>
public class HttpRequest : DynamicRequest
{
    public HttpMethod HttpMethod
    {
        get => (HttpMethod)GetProperty("HttpMethod") ?? System.Net.Http.HttpMethod.Get;
        set => SetProperty("HttpMethod", value);
    }

    public string Route
    {
        get => (string)GetProperty("Route") ?? "";
        set => SetProperty("Route", value);
    }

    public HttpRequest() : base()
    {
        Metadata.Method = "GET";
        Metadata.ContentType = "application/json";
    }

    public HttpRequest(string route, HttpMethod httpMethod) : base(route, httpMethod.Method)
    {
        Route = route;
        HttpMethod = httpMethod;
        Metadata.ContentType = "application/json";
    }
}

/// <summary>
/// Strongly-typed HTTP request
/// </summary>
public class HttpRequest<TResponse> : HttpRequest, IRequest<TResponse>
    where TResponse : class, IResponse
{
    public HttpRequest() : base()
    {
    }

    public HttpRequest(string route, HttpMethod httpMethod) : base(route, httpMethod)
    {
    }
}

/// <summary>
/// gRPC-specific request
/// </summary>
public class GrpcRequest : DynamicRequest
{
    public string ServiceName
    {
        get => (string)GetProperty("ServiceName") ?? "";
        set => SetProperty("ServiceName", value);
    }

    public string MethodName
    {
        get => (string)GetProperty("MethodName") ?? "";
        set => SetProperty("MethodName", value);
    }

    public GrpcRequest() : base()
    {
        Metadata.ContentType = "application/grpc";
    }

    public GrpcRequest(string serviceName, string methodName) : this()
    {
        ServiceName = serviceName;
        MethodName = methodName;
        Metadata.Route = $"/{serviceName}/{methodName}";
        Metadata.Method = methodName;
    }
}

/// <summary>
/// Strongly-typed gRPC request
/// </summary>
public class GrpcRequest<TResponse> : GrpcRequest, IRequest<TResponse>
    where TResponse : class, IResponse
{
    public GrpcRequest() : base()
    {
    }

    public GrpcRequest(string serviceName, string methodName) : base(serviceName, methodName)
    {
    }
}

/// <summary>
/// Request metadata implementation
/// </summary>
internal class RequestMetadata : IRequestMetadata
{
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string ContentType { get; set; } = "application/json";
    public string Route { get; set; } = "/";
    public string Method { get; set; } = "GET";
    public string? CorrelationId { get; set; }
    public TimeSpan? Timeout { get; set; }
    public Dictionary<string, object> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public RetryPolicy? RetryPolicy { get; set; }
}
