namespace HashtApp.Soft.Client.Utilities;
public interface IHttpClientService : IScopeDependency
{
    Task<TResponse> SendAsync<TResponse>(
        IRequest request,
        ResponseWrapperProviderType wrapperType = ResponseWrapperProviderType.AcceptedMessage,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null,
        string contentType = "application/json")
        where TResponse : class
        ;

    Task<TResponse> SendFormAsync<TResponse>(
            IRequest request,
            ResponseWrapperProviderType wrapperType = ResponseWrapperProviderType.AcceptedMessage,
            string clientName = "",
            Dictionary<string, string>? customHeaders = null)
            where TResponse : class
            ;

    void AddCustomHeaders(HttpRequestMessage requestMessage, Dictionary<string, string> customHeaders);
}
