namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Dynamic response implementation supporting flexible property handling
/// Base class for all response types (HTTP, gRPC, etc.)
/// </summary>
public class DynamicResponse : IResponse
{
    private readonly Dictionary<string, object?> _properties;
    private readonly ResponseMetadata _metadata;

    public Guid Id { get; set; }
    public IResponseMetadata Metadata => _metadata;
    public DateTime CreatedAt { get; set; }
    public bool IsSuccess => Metadata.StatusCode >= 200 && Metadata.StatusCode < 300;

    public DynamicResponse()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        _properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        _metadata = new ResponseMetadata();
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
/// Strongly-typed dynamic response with specific data type
/// </summary>
public class DynamicResponse<TData> : DynamicResponse, IResponse<TData>
    where TData : class
{
    public TData? Data { get; set; }

    public DynamicResponse() : base()
    {
    }

    public DynamicResponse(TData? data) : this()
    {
        Data = data;
        Metadata.StatusCode = data != null ? 200 : 204;
    }

    public DynamicResponse(int statusCode, string message) : this()
    {
        Metadata.StatusCode = statusCode;
        Metadata.Message = message;
    }

    public DynamicResponse(int statusCode, TData? data, string? message = null) : this()
    {
        Metadata.StatusCode = statusCode;
        Data = data;
        Metadata.Message = message;
    }
}

/// <summary>
/// HTTP-specific response
/// </summary>
public class HttpResponse : DynamicResponse
{
    public int HttpStatusCode
    {
        get => Metadata.StatusCode;
        set => Metadata.StatusCode = value;
    }

    public HttpResponse() : base()
    {
        Metadata.ContentType = "application/json";
    }

    public HttpResponse(int statusCode) : this()
    {
        HttpStatusCode = statusCode;
    }
}

/// <summary>
/// Strongly-typed HTTP response
/// </summary>
public class HttpResponse<TData> : DynamicResponse<TData>
    where TData : class
{
    public HttpResponse() : base()
    {
        Metadata.ContentType = "application/json";
    }

    public HttpResponse(TData? data) : base(data)
    {
        Metadata.ContentType = "application/json";
    }

    public HttpResponse(int statusCode, TData? data) : base(statusCode, data)
    {
        Metadata.ContentType = "application/json";
    }
}

/// <summary>
/// gRPC-specific response
/// </summary>
public class GrpcResponse : DynamicResponse
{
    public int GrpcStatusCode
    {
        get => Metadata.StatusCode;
        set => Metadata.StatusCode = value;
    }

    public GrpcResponse() : base()
    {
        Metadata.ContentType = "application/grpc";
    }

    public GrpcResponse(int statusCode) : this()
    {
        GrpcStatusCode = statusCode;
    }
}

/// <summary>
/// Strongly-typed gRPC response
/// </summary>
public class GrpcResponse<TData> : DynamicResponse<TData>
    where TData : class
{
    public GrpcResponse() : base()
    {
        Metadata.ContentType = "application/grpc";
    }

    public GrpcResponse(TData? data) : base(data)
    {
        Metadata.ContentType = "application/grpc";
    }

    public GrpcResponse(int statusCode, TData? data) : base(statusCode, data)
    {
        Metadata.ContentType = "application/grpc";
    }
}

/// <summary>
/// Response metadata implementation
/// </summary>
internal class ResponseMetadata : IResponseMetadata
{
    public int StatusCode { get; set; } = 200;
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string ContentType { get; set; } = "application/json";
    public string? Message { get; set; }
    public string? CorrelationId { get; set; }
    public List<ResponseError> Errors { get; } = new();
    public TimeSpan? ElapsedTime { get; set; }
    public Dictionary<string, object> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public bool IsCached { get; set; } = false;
    public List<ValidationError> ValidationErrors { get; } = new();
}
