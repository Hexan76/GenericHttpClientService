using Corevia.Transporter.Abstractions;

namespace Corevia.Transporter.GRPC;

/// <summary>
/// gRPC transport implementation of ITransportClient
/// Provides gRPC-based request/response handling for all dynamic request types
/// </summary>
public class GrpcTransportClient : ITransportClient
{
    private readonly GrpcClientServiceOptions _options;
    private readonly Dictionary<string, object> _clientCache;

    public string TransportName => "Grpc";

    public GrpcTransportClient(GrpcClientServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _clientCache = new Dictionary<string, object>();
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IRequest request,
        TransportOptions? options = null)
        where TResponse : class, IResponse
    {
        options ??= new TransportOptions();

        try
        {
            // Set metadata from options
            if (options.CorrelationId != null)
            {
                request.Metadata.CorrelationId = options.CorrelationId;
            }

            if (options.Timeout.HasValue)
            {
                request.Metadata.Timeout = options.Timeout;
            }

            // Merge metadata headers
            if (options.Metadata.Any())
            {
                foreach (var kvp in options.Metadata)
                {
                    request.Metadata.Headers[kvp.Key] = kvp.Value;
                }
            }

            // Extract service and method information
            var (serviceName, methodName) = ExtractServiceAndMethod(request);

            // TODO: Implement actual gRPC call logic
            // This is a placeholder that returns a mock response
            var startTime = DateTime.UtcNow;

            var response = Activator.CreateInstance<TResponse>();

            if (response is DynamicResponse dynamicResponse)
            {
                dynamicResponse.Metadata.StatusCode = 200;
                dynamicResponse.Metadata.Message = "gRPC call successful (placeholder)";
                dynamicResponse.Metadata.CorrelationId = request.Metadata.CorrelationId;
                dynamicResponse.Metadata.ElapsedTime = DateTime.UtcNow - startTime;
                dynamicResponse.Metadata.ContentType = "application/grpc";
            }

            return response;
        }
        catch (Exception ex)
        {
            var errorResponse = Activator.CreateInstance<TResponse>();
            if (errorResponse is DynamicResponse dynamicErrorResponse)
            {
                dynamicErrorResponse.Metadata.StatusCode = 500;
                dynamicErrorResponse.Metadata.Message = $"gRPC transport error: {ex.Message}";
                dynamicErrorResponse.Metadata.Errors.Add(new ResponseError
                {
                    Code = "GRPC_ERROR",
                    Message = ex.Message,
                    Severity = ErrorSeverity.Error,
                    StackTrace = ex.StackTrace
                });
            }

            return errorResponse;
        }
    }

    public async Task<IResponse> SendAsync(
        IRequest request,
        Type responseType,
        TransportOptions? options = null)
    {
        options ??= new TransportOptions();

        try
        {
            // Set metadata from options
            if (options.CorrelationId != null)
            {
                request.Metadata.CorrelationId = options.CorrelationId;
            }

            if (options.Timeout.HasValue)
            {
                request.Metadata.Timeout = options.Timeout;
            }

            // Extract service and method information
            var (serviceName, methodName) = ExtractServiceAndMethod(request);

            // TODO: Implement actual gRPC call logic
            var startTime = DateTime.UtcNow;

            var response = Activator.CreateInstance(responseType) as IResponse
                ?? throw new InvalidOperationException($"Cannot create instance of {responseType.Name}");

            response.Metadata.StatusCode = 200;
            response.Metadata.Message = "gRPC call successful (placeholder)";
            response.Metadata.CorrelationId = request.Metadata.CorrelationId;
            response.Metadata.ElapsedTime = DateTime.UtcNow - startTime;
            response.Metadata.ContentType = "application/grpc";

            return response;
        }
        catch (Exception ex)
        {
            var response = new DynamicResponse
            {
                Metadata =
                {
                    StatusCode = 500,
                    Message = $"gRPC transport error: {ex.Message}"
                }
            };

            response.Metadata.Errors.Add(new ResponseError
            {
                Code = "GRPC_ERROR",
                Message = ex.Message,
                Severity = ErrorSeverity.Error,
                StackTrace = ex.StackTrace
            });

            return response;
        }
    }

    public Task<bool> IsHealthyAsync()
    {
        // TODO: Implement health check for gRPC transport
        return Task.FromResult(true);
    }

    private (string ServiceName, string MethodName) ExtractServiceAndMethod(IRequest request)
    {
        if (request is GrpcRequest grpcRequest)
        {
            return (grpcRequest.ServiceName, grpcRequest.MethodName);
        }

        // Try to parse from route
        var route = request.Metadata.Route;
        var method = request.Metadata.Method;

        // Expected format: /ServiceName/MethodName or similar
        var parts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var serviceName = parts.Length > 0 ? parts[0] : "Unknown";
        var methodName = parts.Length > 1 ? parts[1] : method;

        return (serviceName, methodName);
    }
}
