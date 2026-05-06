namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Factory for creating and managing transport clients
/// Supports multiple simultaneous connections and per-request transport selection
/// </summary>
public interface ITransportClientFactory
{
    /// <summary>
    /// Gets a transport client by transport name (e.g., "Http", "Grpc")
    /// </summary>
    /// <param name="transportName">The name of the transport</param>
    /// <returns>The transport client, or null if not found</returns>
    ITransportClient? GetClient(string transportName);

    /// <summary>
    /// Gets all available transport clients
    /// </summary>
    IEnumerable<ITransportClient> GetAllClients();

    /// <summary>
    /// Gets the default transport client
    /// </summary>
    ITransportClient GetDefaultClient();

    /// <summary>
    /// Registers a transport client
    /// </summary>
    void RegisterClient(string transportName, ITransportClient client);
}

/// <summary>
/// Selector for choosing which transport to use for a given request
/// </summary>
public interface ITransportSelector
{
    /// <summary>
    /// Selects a transport for the given request
    /// Enables per-request customization of which transport (HTTP or gRPC) to use
    /// </summary>
    /// <param name="request">The request to send</param>
    /// <param name="context">Optional context for selection (e.g., user preferences, feature flags)</param>
    /// <returns>The name of the transport to use (e.g., "Http", "Grpc")</returns>
    string SelectTransport(IRequest request, TransportSelectionContext? context = null);
}

/// <summary>
/// Context for transport selection decisions
/// Allows customization of transport choice per-request
/// </summary>
public class TransportSelectionContext
{
    /// <summary>
    /// User or request ID for making selection decisions
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Custom attributes for selection logic (e.g., feature flags, preferences)
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();

    /// <summary>
    /// Preferred transport, if any
    /// </summary>
    public string? PreferredTransport { get; set; }

    /// <summary>
    /// Whether to fail if preferred transport is not available
    /// </summary>
    public bool FailIfPreferredUnavailable { get; set; } = false;
}
