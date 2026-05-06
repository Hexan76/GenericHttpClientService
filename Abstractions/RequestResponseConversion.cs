namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Utility methods for converting between different request/response types
/// </summary>
public static class RequestResponseConversion
{
    /// <summary>
    /// Converts a legacy IRequest to a DynamicRequest
    /// </summary>
    public static DynamicRequest ConvertToDynamic(this IRequest request)
    {
        if (request is DynamicRequest dynamicRequest)
            return dynamicRequest;

        var result = new DynamicRequest();
        
        // Copy basic properties
        result.Id = request.Id;
        result.CreatedAt = request.CreatedAt;

        // Copy metadata
        result.Metadata.CorrelationId = request.Metadata.CorrelationId;
        result.Metadata.ContentType = request.Metadata.ContentType;
        result.Metadata.Route = request.Metadata.Route;
        result.Metadata.Method = request.Metadata.Method;
        result.Metadata.Timeout = request.Metadata.Timeout;

        // Copy headers
        foreach (var kvp in request.Metadata.Headers)
        {
            result.Metadata.Headers[kvp.Key] = kvp.Value;
        }

        // Copy attributes
        foreach (var kvp in request.Metadata.Attributes)
        {
            result.Metadata.Attributes[kvp.Key] = kvp.Value;
        }

        // Copy all dynamic properties
        foreach (var kvp in request.GetAllProperties())
        {
            result.SetProperty(kvp.Key, kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// Converts a strongly-typed request to a DynamicRequest while preserving the response type.
    /// </summary>
    public static DynamicRequest<TResponse> ConvertToDynamic<TResponse>(this IRequest<TResponse> request)
        where TResponse : class, IResponse
    {
        if (request is DynamicRequest<TResponse> dynamicRequest)
            return dynamicRequest;

        var result = new DynamicRequest<TResponse>();

        result.Id = request.Id;
        result.CreatedAt = request.CreatedAt;

        result.Metadata.CorrelationId = request.Metadata.CorrelationId;
        result.Metadata.ContentType = request.Metadata.ContentType;
        result.Metadata.Route = request.Metadata.Route;
        result.Metadata.Method = request.Metadata.Method;
        result.Metadata.Timeout = request.Metadata.Timeout;

        foreach (var kvp in request.Metadata.Headers)
        {
            result.Metadata.Headers[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in request.Metadata.Attributes)
        {
            result.Metadata.Attributes[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in request.GetAllProperties())
        {
            result.SetProperty(kvp.Key, kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// Converts a legacy IResponse to a DynamicResponse
    /// </summary>
    public static DynamicResponse ConvertToDynamic(this IResponse response)
    {
        if (response is DynamicResponse dynamicResponse)
            return dynamicResponse;

        var result = new DynamicResponse();

        // Copy basic properties
        result.Id = response.Id;
        result.CreatedAt = response.CreatedAt;

        // Copy metadata
        result.Metadata.StatusCode = response.Metadata.StatusCode;
        result.Metadata.ContentType = response.Metadata.ContentType;
        result.Metadata.Message = response.Metadata.Message;
        result.Metadata.CorrelationId = response.Metadata.CorrelationId;
        result.Metadata.ElapsedTime = response.Metadata.ElapsedTime;
        result.Metadata.ReceivedAt = response.Metadata.ReceivedAt;
        result.Metadata.IsCached = response.Metadata.IsCached;

        // Copy headers
        foreach (var kvp in response.Metadata.Headers)
        {
            result.Metadata.Headers[kvp.Key] = kvp.Value;
        }

        // Copy errors
        result.Metadata.Errors.AddRange(response.Metadata.Errors);

        // Copy validation errors
        result.Metadata.ValidationErrors.AddRange(response.Metadata.ValidationErrors);

        // Copy attributes
        foreach (var kvp in response.Metadata.Attributes)
        {
            result.Metadata.Attributes[kvp.Key] = kvp.Value;
        }

        // Copy all dynamic properties
        foreach (var kvp in response.GetAllProperties())
        {
            result.SetProperty(kvp.Key, kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// Converts a strongly-typed response to a DynamicResponse while preserving the response payload.
    /// </summary>
    public static DynamicResponse<TData> ConvertToDynamic<TData>(this IResponse<TData> response)
        where TData : class
    {
        if (response is DynamicResponse<TData> dynamicResponse)
            return dynamicResponse;

        var result = new DynamicResponse<TData>(response.Data);

        // Copy basic properties
        result.Id = response.Id;
        result.CreatedAt = response.CreatedAt;

        // Copy metadata
        result.Metadata.StatusCode = response.Metadata.StatusCode;
        result.Metadata.ContentType = response.Metadata.ContentType;
        result.Metadata.Message = response.Metadata.Message;
        result.Metadata.CorrelationId = response.Metadata.CorrelationId;
        result.Metadata.ElapsedTime = response.Metadata.ElapsedTime;
        result.Metadata.ReceivedAt = response.Metadata.ReceivedAt;
        result.Metadata.IsCached = response.Metadata.IsCached;

        // Copy headers
        foreach (var kvp in response.Metadata.Headers)
        {
            result.Metadata.Headers[kvp.Key] = kvp.Value;
        }

        // Copy errors
        result.Metadata.Errors.AddRange(response.Metadata.Errors);

        // Copy validation errors
        result.Metadata.ValidationErrors.AddRange(response.Metadata.ValidationErrors);

        // Copy attributes
        foreach (var kvp in response.Metadata.Attributes)
        {
            result.Metadata.Attributes[kvp.Key] = kvp.Value;
        }

        // Copy all dynamic properties
        foreach (var kvp in response.GetAllProperties())
        {
            result.SetProperty(kvp.Key, kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// Wraps an HTTP response with AcceptMessage wrapper
    /// </summary>
    public static DynamicResponse<T> WrapAcceptMessage<T>(this T data, Guid correlationId)
        where T : class
    {
        var response = new DynamicResponse<T>(200, data);
        response.Metadata.CorrelationId = correlationId.ToString();
        response.Metadata.Message = "Success";
        return response;
    }

    /// <summary>
    /// Wraps a response with MessageContract
    /// </summary>
    public static DynamicResponse WrapMessageContract(
        this DynamicResponse response,
        string message,
        System.Net.HttpStatusCode statusCode,
        int severity = 0)
    {
        response.Metadata.Message = message;
        response.Metadata.StatusCode = (int)statusCode;
        response.Metadata.Attributes["Severity"] = severity;
        return response;
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static DynamicResponse CreateErrorResponse(
        string message,
        string errorCode = "INTERNAL_ERROR",
        int statusCode = 500,
        string? correlationId = null)
    {
        var response = new DynamicResponse();
        response.Metadata.StatusCode = statusCode;
        response.Metadata.Message = message;
        response.Metadata.CorrelationId = correlationId;
        response.Metadata.Errors.Add(new ResponseError
        {
            Code = errorCode,
            Message = message,
            Severity = ErrorSeverity.Error
        });
        return response;
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static DynamicResponse CreateValidationErrorResponse(
        IEnumerable<ValidationError> validationErrors,
        string? correlationId = null)
    {
        var response = new DynamicResponse();
        response.Metadata.StatusCode = 400;
        response.Metadata.Message = "Validation failed";
        response.Metadata.CorrelationId = correlationId;
        response.Metadata.ValidationErrors.AddRange(validationErrors);
        return response;
    }

    /// <summary>
    /// Merges transport options into request metadata
    /// </summary>
    public static void MergeOptions(this IRequest request, TransportOptions options)
    {
        if (!string.IsNullOrEmpty(options.CorrelationId))
        {
            request.Metadata.CorrelationId = options.CorrelationId;
        }

        if (options.Timeout.HasValue)
        {
            request.Metadata.Timeout = options.Timeout;
        }

        if (options.RetryPolicy != null)
        {
            request.Metadata.RetryPolicy = options.RetryPolicy;
        }

        foreach (var kvp in options.Metadata)
        {
            request.Metadata.Headers[kvp.Key] = kvp.Value;
        }
    }
}