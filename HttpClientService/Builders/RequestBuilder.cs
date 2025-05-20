using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HashtApp.Soft.Client.Utilities;

public class RequestBuilder : IRequestBuilder
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public HttpRequestMessage Build<TRequest>(TRequest request, string contentType) where TRequest : IRequest
    {
        var (finalRoute, queryParams, bodyContent) = ResolveRequestFields(request);
        var requestUri = BuildRequestUri(finalRoute, queryParams);
        var message = new HttpRequestMessage
        {
            Method = request.HttpMethod,
            RequestUri = requestUri
        };

        if (request.HttpMethod == HttpMethod.Post ||
            request.HttpMethod == HttpMethod.Put ||
            request.HttpMethod == HttpMethod.Patch)
        {
            if (bodyContent.Count > 0)
            {
                var json = JsonConvert.SerializeObject(bodyContent, _jsonSerializerSettings);
                message.Content = new StringContent(json, Encoding.UTF8, contentType);
            }
        }

        return message;
    }

    private Uri BuildRequestUri(string route, Dictionary<string, string> queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
            return new Uri(route, UriKind.Relative);

        var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;
        return new Uri($"{route}?{queryString}", UriKind.Relative);
    }

    public (string finalRoute, Dictionary<string, string> queryParams, Dictionary<string, object> bodyContent)
        ResolveRequestFields<TRequest>(TRequest request) where TRequest : IRequest
    {
        var method = request.HttpMethod;
        var routeTemplate = request.Route;

        // Step 1: Exclude IRequest props (interface properties)
        var excluded = typeof(IRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet();

        // Step 2: Get all properties from the request and exclude IRequest properties
        var props = request.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !excluded.Contains(p.Name))
            .Select(p => (p.Name, Value: p.GetValue(request)))
            .Where(p => p.Value != null)
            .ToList();

        // Step 3: Create a fast lookup dictionary for properties
        var propDict = props.ToDictionary(p => p.Name, p => p.Value, StringComparer.OrdinalIgnoreCase);

        // Step 4: Replace route placeholders with values from the request properties
        var usedInRoute = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var finalRoute = RouteParamRegex.Replace(routeTemplate, match =>
        {
            var key = match.Groups[1].Value;
            if (propDict.TryGetValue(key, out var val))
            {
                usedInRoute.Add(key);
                return Uri.EscapeDataString(Convert.ToString(val, CultureInfo.InvariantCulture)!);
            }
            return match.Value;
        });

        // Step 5: Get the remaining properties (those not used in the route)
        var remaining = propDict
            .Where(p => !usedInRoute.Contains(p.Key))
            .ToList();

        // Step 6: Apply properties to query parameters for GET method or body for POST/PUT/PATCH
        var queryParams = method == HttpMethod.Get
            ? remaining.ToDictionary(
                p => p.Key,
                p => Convert.ToString(p.Value, CultureInfo.InvariantCulture)!)
            : new Dictionary<string, string>();

        var bodyContent = method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch
            ? remaining.ToDictionary(p => p.Key, p => p.Value)
            : new Dictionary<string, object>();

        return (finalRoute, queryParams, bodyContent);
    }

    private static readonly Regex RouteParamRegex = new(@"{(\w+)}", RegexOptions.Compiled);

}
