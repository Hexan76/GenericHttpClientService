namespace HashtApp.Soft.Client.Utilities;

public interface IHttpInterceptorService
{
    void OnBeforeSend(HttpRequestMessage request);

    void OnAfterSend(HttpResponseMessage response);

    void OnException(Exception ex);
    
}
