namespace HashtApp.Soft.Client.Utilities;

public interface IResponseHandlerFactory : IScopeDependency
{
    IResponseHandler GetHandlerFor(Type responseType, ResponseWrapperProviderType? wrapperType);
}
