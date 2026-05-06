namespace Corevia.Transporter.Http;
public interface IRequest
{
    HttpMethod HttpMethod { get; }
    string Route { get; }

}
