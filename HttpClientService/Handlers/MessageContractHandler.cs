using Newtonsoft.Json.Serialization;

namespace HashtApp.Soft.Client.Utilities;

public class MessageContractHandler : IResponseHandler<MessageContract>
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public MessageContractHandler(JsonSerializerSettings jsonSerializerSettings)
    {
        this._jsonSerializerSettings = jsonSerializerSettings;
    }

    public async Task<MessageContract> HandleAsync(HttpResponseMessage response)
    {
        Console.WriteLine("MessageContract Resolver Response");
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<MessageContract>(content, _jsonSerializerSettings);
    }

    async Task<object> IResponseHandler.HandleAsync(HttpResponseMessage response, Type targetType)
    {
        return await HandleAsync(response);
    }
}
