namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Enhanced request interface with metadata support for multiple transports
/// </summary>
public interface IRequest : IDynamicObject
{
    /// <summary>
    /// Unique request identifier
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Request metadata (headers, custom properties, etc.)
    /// </summary>
    IRequestMetadata Metadata { get; }

    /// <summary>
    /// Request creation timestamp
    /// </summary>
    DateTime CreatedAt { get; }
}

/// <summary>
/// Enhanced request interface with response type information
/// </summary>
public interface IRequest<TResponse> : IRequest
    where TResponse : class, IResponse
{
}

/// <summary>
/// Interface for dynamic object support (allows setting/getting properties dynamically)
/// </summary>
public interface IDynamicObject
{
    /// <summary>
    /// Gets a dynamic property value
    /// </summary>
    object? GetProperty(string name);

    /// <summary>
    /// Sets a dynamic property value
    /// </summary>
    void SetProperty(string name, object? value);

    /// <summary>
    /// Gets all dynamic properties
    /// </summary>
    IReadOnlyDictionary<string, object?> GetAllProperties();

    /// <summary>
    /// Checks if property exists
    /// </summary>
    bool HasProperty(string name);
}

/// <summary>
/// Request metadata including headers, content type, routing info, etc.
/// </summary>
public interface IRequestMetadata
{
    /// <summary>
    /// Request headers/metadata (HTTP headers, gRPC metadata, etc.)
    /// </summary>
    Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Content type (application/json, application/x-protobuf, etc.)
    /// </summary>
    string ContentType { get; set; }

    /// <summary>
    /// Request path/route
    /// </summary>
    string Route { get; set; }

    /// <summary>
    /// Request method/action (GET, POST, RPC name, etc.)
    /// </summary>
    string Method { get; set; }

    /// <summary>
    /// Correlation ID for tracing across services
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    /// Request timeout
    /// </summary>
    TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Custom attributes for request processing
    /// </summary>
    Dictionary<string, object> Attributes { get; }

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    RetryPolicy? RetryPolicy { get; set; }
}

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryPolicy
{
    /// <summary>
    /// Maximum number of retries
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial delay between retries
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Maximum delay between retries
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Backoff multiplier (exponential backoff)
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// HTTP status codes that should trigger retry
    /// </summary>
    public HashSet<int> RetryableStatusCodes { get; set; } = new() { 408, 429, 500, 502, 503, 504 };

    /// <summary>
    /// gRPC status codes that should trigger retry
    /// </summary>
    public HashSet<int> RetryableGrpcCodes { get; set; } = new() { 1, 2, 4, 8, 13, 14 }; // CANCELLED, UNKNOWN, DEADLINE_EXCEEDED, RESOURCE_EXHAUSTED, INTERNAL, UNAVAILABLE
}
