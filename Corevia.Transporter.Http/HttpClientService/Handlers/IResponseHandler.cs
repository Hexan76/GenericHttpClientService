namespace Corevia.Transporter.Http;

public interface IResponseHandler
{
    Task<object> HandleAsync(HttpResponseMessage response, Type targetType);
}