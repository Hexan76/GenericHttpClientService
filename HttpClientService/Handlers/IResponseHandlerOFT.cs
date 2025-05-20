namespace HashtApp.Soft.Client.Utilities;

public interface IResponseHandler<TResponse> : IResponseHandler
{
    Task<TResponse> HandleAsync(HttpResponseMessage response);
}
