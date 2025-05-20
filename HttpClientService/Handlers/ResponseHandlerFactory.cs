using Newtonsoft.Json.Serialization;

namespace HashtApp.Soft.Client.Utilities;

public class ResponseHandlerFactory : IResponseHandlerFactory
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };
    public IResponseHandler GetHandlerFor(Type responseType, ResponseWrapperProviderType? wrapperType)
    {
        switch (wrapperType)
        {
            case ResponseWrapperProviderType.MessageContract:
                return new MessageContractHandler(_jsonSerializerSettings);
            case ResponseWrapperProviderType.AcceptedMessage:
                var handlerType = typeof(AcceptedResponseHandler<>).MakeGenericType(responseType);
                return (IResponseHandler)Activator.CreateInstance(handlerType, _jsonSerializerSettings)!;
            case ResponseWrapperProviderType.Raw:
            default:
                return new DefaultResponseHandler(_jsonSerializerSettings);
        }
    }
}
