namespace Corevia.Transporter.Http;

public interface IRequestResolver : IScopeDependency
{
    (string finalRoute, Dictionary<string, string> queryParams, Dictionary<string, object> bodyContent)
        ResolveRequestFields<TRequest>(TRequest request) where TRequest : IRequest;
}
