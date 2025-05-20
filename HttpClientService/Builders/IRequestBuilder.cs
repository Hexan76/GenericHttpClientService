namespace HashtApp.Soft.Client.Utilities;

public interface IRequestBuilder : IScopeDependency
{
    HttpRequestMessage Build<TRequest>(TRequest request, string contentType)
        where TRequest : IRequest;
}
