namespace Corevia.Transporter.Http;

public interface IResponseHandlerFactory : IScopeDependency
{
    IResponseHandler GetHandlerFor(Type responseType, ResponseWrapperProviderType? wrapperType);
}
