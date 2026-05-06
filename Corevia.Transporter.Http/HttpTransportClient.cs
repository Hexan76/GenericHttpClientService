using Corevia.Transporter.Abstractions;

namespace Corevia.Transporter.Http;

/// <summary>
/// HTTP transport implementation of ITransportClient
/// Wraps the existing IHttpClientService to implement the new transport abstraction
/// Supports both legacy and new dynamic request/response models
/// </summary>
public class HttpTransportClient : ITransportClient
{
    private readonly IHttpClientService _httpClientService;

    public string TransportName => "Http";

    public HttpTransportClient(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IRequest request,
        TransportOptions? options = null)
        where TResponse : class, IResponse
    {
        options ??= new TransportOptions();

        // For HTTP requests, extract HTTP method and route
        var httpMethod = GetHttpMethod(request);
        var route = GetRoute(request);

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

        return await _httpClientService.SendAsync<TResponse>(
            request,
            options.WrapperType,
            options.ConfigurationName,
            request.Metadata.Headers.Any() ? request.Metadata.Headers : null,
            contentType: request.Metadata.ContentType);
    }

    public async Task<IResponse> SendAsync(
        IRequest request,
        Type responseType,
        TransportOptions? options = null)
    {
        options ??= new TransportOptions();

        // For HTTP requests, extract HTTP method and route
        var httpMethod = GetHttpMethod(request);
        var route = GetRoute(request);

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

        // Use reflection to call the generic SendAsync method
        var method = _httpClientService.GetType()
            .GetMethods()
            .FirstOrDefault(m => m.Name == "SendAsync" && m.IsGenericMethodDefinition)?
            .MakeGenericMethod(responseType);

        if (method == null)
        {
            throw new InvalidOperationException($"Cannot find SendAsync method for type {responseType.Name}");
        }

        var task = (Task)method.Invoke(_httpClientService, new object[] 
        { 
            request, 
            options.WrapperType, 
            options.ConfigurationName,
            request.Metadata.Headers.Any() ? request.Metadata.Headers : null,
            request.Metadata.ContentType
        })!;

        await task;

        var resultProperty = task.GetType().GetProperty("Result");
        var result = resultProperty?.GetValue(task) as IResponse;

        return result ?? new DynamicResponse 
        { 
            Metadata = { StatusCode = 500, Message = "Failed to deserialize response" } 
        };
    }

    public Task<bool> IsHealthyAsync()
    {
        // TODO: Implement health check for HTTP transport
        return Task.FromResult(true);
    }

    private HttpMethod GetHttpMethod(IRequest request)
    {
        if (request is HttpRequest httpRequest)
        {
            return httpRequest.HttpMethod;
        }

        // Try to get from metadata
        var methodStr = request.Metadata.Method.ToUpperInvariant();
        return methodStr switch
        {
            "GET" => System.Net.Http.HttpMethod.Get,
            "POST" => System.Net.Http.HttpMethod.Post,
            "PUT" => System.Net.Http.HttpMethod.Put,
            "DELETE" => System.Net.Http.HttpMethod.Delete,
            "PATCH" => new HttpMethod("PATCH"),
            "HEAD" => System.Net.Http.HttpMethod.Head,
            "OPTIONS" => System.Net.Http.HttpMethod.Options,
            _ => System.Net.Http.HttpMethod.Post
        };
    }

    private string GetRoute(IRequest request)
    {
        if (request is HttpRequest httpRequest)
        {
            return httpRequest.Route;
        }

        return request.Metadata.Route ?? "/";
    }
}
