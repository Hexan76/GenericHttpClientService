using Microsoft.AspNetCore.Components.Forms;

namespace HashtApp.Soft.Client.Utilities;

public class FormContentBuilder : IFormContentBuilder
{
    public HttpContent Build(Dictionary<string, object> bodyContent, string contentType)
    {
        if (string.Equals(contentType, "multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var formData = new MultipartFormDataContent();

            foreach (var (key, value) in bodyContent)
            {
                switch (value)
                {
                    case Stream stream:
                        formData.Add(new StreamContent(stream), key, "file"); // fallback filename
                        break;

                    case byte[] bytes:
                        formData.Add(new ByteArrayContent(bytes), key, "file");
                        break;

                    case IBrowserFile browserFile:
                        var fileContent = new StreamContent(browserFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(browserFile.ContentType);
                        formData.Add(fileContent, key, browserFile.Name);
                        break;

                    default:
                        formData.Add(new StringContent(value.ToString() ?? ""), key);
                        break;
                }
            }

            return formData;
        }

        // Fallback to JSON
        var json = JsonConvert.SerializeObject(bodyContent, new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        });
        return new StringContent(json, Encoding.UTF8, contentType);
    }
}
