using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Options;

namespace HashtApp.Soft.Client.Utilities;

public class HttpClientService(
    IHttpClientFactory httpClientFactory,
    IToastService toastService,
    IRequestBuilder requestBuilder,
    IFormContentBuilder formContentBuilder,
    IResponseHandlerFactory responseHandlerFactory,
    IOptions<HttpClientServiceOptions> httpOptions,
    IRequestResolver requestResolver) : IHttpClientService
{
    public async Task<TResponse> SendAsync<TResponse>(
        IRequest request,
        ResponseWrapperProviderType wrapperType = ResponseWrapperProviderType.AcceptedMessage,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null,
        string contentType = "application/json")
        where TResponse : class
    {
        try
        {

            var client = httpClientFactory.CreateClient(string.IsNullOrWhiteSpace(clientName) ? httpOptions.Value.DefaultClientName : clientName);
            var message = requestBuilder.Build(request, contentType);
            AddCustomHeaders(message, customHeaders);

            var response = await client.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var handler = responseHandlerFactory.GetHandlerFor(typeof(TResponse), wrapperType);
            var result = await handler.HandleAsync(response, typeof(TResponse));
            return (TResponse)result;
        }
        catch (Exception ex)
        {
            toastService.Error("httpClient", ex.Message);
            throw;
        }
    }

    public async Task<TResponse> SendFormAsync<TResponse>(
        IRequest request,
        ResponseWrapperProviderType wrapperType = ResponseWrapperProviderType.AcceptedMessage,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null)
        where TResponse : class
    {
        var client = httpClientFactory.CreateClient(string.IsNullOrWhiteSpace(clientName) ? httpOptions.Value.DefaultClientName : clientName);
        var (finalRoute, _, bodyContent) = requestResolver.ResolveRequestFields(request);
        var content = formContentBuilder.Build(bodyContent, "multipart/form-data");

        var message = new HttpRequestMessage(request.HttpMethod, finalRoute) { Content = content };
        AddCustomHeaders(message, customHeaders);

        var response = await client.SendAsync(message);
        response.EnsureSuccessStatusCode();

        var handler = responseHandlerFactory.GetHandlerFor(typeof(TResponse), wrapperType);
        var result = await handler.HandleAsync(response, typeof(TResponse));
        return (TResponse)result;
    }

    public void AddCustomHeaders(HttpRequestMessage message, Dictionary<string, string> customHeaders)
    {
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        if (customHeaders != null)
        {
            foreach (var kvp in customHeaders)
                message.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
        }
    }
}
