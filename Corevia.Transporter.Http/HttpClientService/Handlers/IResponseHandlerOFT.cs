namespace Corevia.Transporter.Http;

public interface IResponseHandler<TResponse> : IResponseHandler
{
    Task<TResponse> HandleAsync(HttpResponseMessage response);
}
