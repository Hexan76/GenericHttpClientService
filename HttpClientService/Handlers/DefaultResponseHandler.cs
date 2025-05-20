namespace HashtApp.Soft.Client.Utilities;

public class DefaultResponseHandler : IResponseHandler
{
    private readonly JsonSerializerSettings _jsonSettings;

    public DefaultResponseHandler(JsonSerializerSettings jsonSettings)
    {
        _jsonSettings = jsonSettings;
    }

    public async Task<object> HandleAsync(HttpResponseMessage response, Type targetType)
    {
        Console.WriteLine("Default Resolver Response");
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject(content, targetType, _jsonSettings)!;
    }
}
