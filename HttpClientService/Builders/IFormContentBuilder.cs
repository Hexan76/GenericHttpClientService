namespace HashtApp.Soft.Client.Utilities;

public interface IFormContentBuilder : IScopeDependency
{
    HttpContent Build(Dictionary<string, object> bodyContent, string contentType);
}
