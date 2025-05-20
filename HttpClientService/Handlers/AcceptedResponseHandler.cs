namespace HashtApp.Soft.Client.Utilities;

public class AcceptedResponseHandler<TResponse> : IResponseHandler<TResponse>
    where TResponse : class
{
    private readonly JsonSerializerSettings _jsonSettings;

    public AcceptedResponseHandler(JsonSerializerSettings jsonSettings)
    {
        _jsonSettings = jsonSettings;
    }

    public async Task<TResponse> HandleAsync(HttpResponseMessage response)
    {
        Console.WriteLine("Acceptes Resolver Response");
        var content = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<AcceptMessage<TResponse>>(content, _jsonSettings);
        return wrapped.Data;
    }

    async Task<object> IResponseHandler.HandleAsync(HttpResponseMessage response, Type targetType)
    {
        return await HandleAsync(response);
    }
}
