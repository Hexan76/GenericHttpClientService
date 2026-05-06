namespace Corevia.Transporter.Http;

public interface IFormContentBuilder : IScopeDependency
{
    HttpContent Build(Dictionary<string, object> bodyContent, string contentType);
}
