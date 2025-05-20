namespace HashtApp.Soft.Client.Utilities;

public interface IResponseHandler
{
    Task<object> HandleAsync(HttpResponseMessage response, Type targetType);
}