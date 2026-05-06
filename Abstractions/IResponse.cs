namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Enhanced response interface with metadata support for multiple transports
/// </summary>
public interface IResponse : IDynamicObject
{
    /// <summary>
    /// Response ID (should match the request ID)
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Response metadata (status, headers, etc.)
    /// </summary>
    IResponseMetadata Metadata { get; }

    /// <summary>
    /// Response creation timestamp
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Indicates if the response is successful
    /// </summary>
    bool IsSuccess { get; }
}

/// <summary>
/// Enhanced response interface with specific data type
/// </summary>
public interface IResponse<TData> : IResponse
    where TData : class
{
    /// <summary>
    /// The actual response data
    /// </summary>
    TData? Data { get; }
}

/// <summary>
/// Response metadata including status, headers, timing, etc.
/// </summary>
public interface IResponseMetadata
{
    /// <summary>
    /// HTTP status code or gRPC status code
    /// </summary>
    int StatusCode { get; set; }

    /// <summary>
    /// Response headers/metadata (HTTP headers, gRPC metadata, etc.)
    /// </summary>
    Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Content type of the response (application/json, application/x-protobuf, etc.)
    /// </summary>
    string ContentType { get; set; }

    /// <summary>
    /// Response message/description
    /// </summary>
    string? Message { get; set; }

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    /// Errors that occurred during processing
    /// </summary>
    List<ResponseError> Errors { get; }

    /// <summary>
    /// Response duration/elapsed time
    /// </summary>
    TimeSpan? ElapsedTime { get; set; }

    /// <summary>
    /// Custom attributes in response
    /// </summary>
    Dictionary<string, object> Attributes { get; }

    /// <summary>
    /// When the response was received
    /// </summary>
    DateTime ReceivedAt { get; set; }

    /// <summary>
    /// Whether response was cached
    /// </summary>
    bool IsCached { get; set; }

    /// <summary>
    /// Validations that failed (if any)
    /// </summary>
    List<ValidationError> ValidationErrors { get; }
}

/// <summary>
/// Represents an error in the response
/// </summary>
public class ResponseError
{
    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Detailed error information
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Error severity level
    /// </summary>
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;

    /// <summary>
    /// Inner exception information (for debugging)
    /// </summary>
    public string? StackTrace { get; set; }
}

/// <summary>
/// Validation error details
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Property/field name that failed validation
    /// </summary>
    public string PropertyName { get; set; } = "";

    /// <summary>
    /// Validation error message
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Attempted value
    /// </summary>
    public object? AttemptedValue { get; set; }

    /// <summary>
    /// Validation error code
    /// </summary>
    public string ErrorCode { get; set; } = "";
}

/// <summary>
/// Error severity levels
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Information level
    /// </summary>
    Information = 0,

    /// <summary>
    /// Warning level
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error level
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical error level
    /// </summary>
    Critical = 3
}
