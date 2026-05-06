namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Strongly-typed transport model pairing a request and response.
/// This can be used for scenarios where both request and response payloads
/// need to be passed through processing pipelines in a single object.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public readonly struct TransportModel<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    public TransportModel(TRequest request, TResponse response)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    public TRequest Request { get; }

    public TResponse Response { get; }

    public static TransportModel<TRequest, TResponse> Create(TRequest request, TResponse response)
        => new(request, response);
}
