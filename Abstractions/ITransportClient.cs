namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Transport-agnostic client interface for sending requests over any transport (HTTP, gRPC, etc.)
/// Supports multiple simultaneous connections and per-request transport selection
/// </summary>
public interface ITransportClient
{
    /// <summary>
    /// Sends a request and returns a strongly-typed response
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="request">The request to send</param>
    /// <param name="options">Optional transport-specific options</param>
    /// <returns>The response</returns>
    Task<TResponse> SendAsync<TResponse>(
        IRequest request,
        TransportOptions? options = null)
        where TResponse : class, IResponse;

    /// <summary>
    /// Sends a request with dynamic response handling
    /// </summary>
    /// <param name="request">The request to send</param>
    /// <param name="responseType">The expected response type</param>
    /// <param name="options">Optional transport-specific options</param>
    /// <returns>The response as dynamic object</returns>
    Task<IResponse> SendAsync(
        IRequest request,
        Type responseType,
        TransportOptions? options = null);

    /// <summary>
    /// Gets the name of this transport (e.g., "Http", "Grpc")
    /// </summary>
    string TransportName { get; }

    /// <summary>
    /// Indicates if this transport client is available/healthy
    /// </summary>
    Task<bool> IsHealthyAsync();
}

/// <summary>
/// Configuration options for transport operations
/// </summary>
public class TransportOptions
{
    /// <summary>
    /// Named configuration for this transport instance (supports multiple connections)
    /// </summary>
    public string ConfigurationName { get; set; } = "default";

    /// <summary>
    /// Timeout for the request
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Custom metadata/headers for the transport
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Transport-specific response wrapper type (e.g., AcceptedMessage, Raw)
    /// </summary>
    public ResponseWrapperProviderType WrapperType { get; set; } = ResponseWrapperProviderType.AcceptedMessage;

    /// <summary>
    /// Whether to use cached response if available
    /// </summary>
    public bool AllowCached { get; set; } = false;

    /// <summary>
    /// Cache duration (if caching is enabled)
    /// </summary>
    public TimeSpan? CacheDuration { get; set; }

    /// <summary>
    /// Custom correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Retry policy for this request
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }
}
