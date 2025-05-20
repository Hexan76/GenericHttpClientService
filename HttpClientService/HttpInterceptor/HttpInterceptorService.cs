namespace HashtApp.Soft.Client.Utilities;

public class HttpInterceptorService(IToastService toastService) : IHttpInterceptorService, IScopeDependency
{
    public virtual void OnBeforeSend(HttpRequestMessage request)
    {
#if DEBUG
        Console.WriteLine($"[Request] {request.Method} {request.RequestUri}");
#endif
    }

    public virtual void OnAfterSend(HttpResponseMessage response)
    {
#if DEBUG
        Console.WriteLine($"[Response] {response.StatusCode}");
#endif
    }

    public virtual void OnException(Exception ex)
    {
        //Console.WriteLine($"[Error] {ex.Message}");
#if DEBUG
        var debuggerMessage = System.Text.Json.JsonSerializer.Serialize($"{ex.Message}\r\n{ex.InnerException.Message}\r\n{ex.StackTrace}");
        toastService.Error("Exception Error", debuggerMessage);
#endif
        toastService.Error("Error", ex.Message);
        // ShowToast("An error occurred while processing your request.");
    }

}
