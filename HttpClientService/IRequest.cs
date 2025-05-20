namespace HashtApp.Soft.Client.Utilities;
public interface IRequest
{
    HttpMethod HttpMethod { get; }
    string Route { get; }

}
