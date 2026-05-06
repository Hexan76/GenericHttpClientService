using Corevia.Transporter.Abstractions;

namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Default implementation of ITransportClientFactory
/// Manages multiple transport client instances
/// </summary>
public class TransportClientFactory : ITransportClientFactory
{
    private readonly Dictionary<string, ITransportClient> _clients;
    private string _defaultTransport = "Http";

    public TransportClientFactory()
    {
        _clients = new Dictionary<string, ITransportClient>(StringComparer.OrdinalIgnoreCase);
    }

    public ITransportClient? GetClient(string transportName)
    {
        _clients.TryGetValue(transportName, out var client);
        return client;
    }

    public IEnumerable<ITransportClient> GetAllClients()
    {
        return _clients.Values;
    }

    public ITransportClient GetDefaultClient()
    {
        var client = GetClient(_defaultTransport);
        if (client == null)
        {
            throw new InvalidOperationException($"Default transport '{_defaultTransport}' is not registered");
        }
        return client;
    }

    public void RegisterClient(string transportName, ITransportClient client)
    {
        if (string.IsNullOrWhiteSpace(transportName))
            throw new ArgumentException("Transport name cannot be empty", nameof(transportName));

        if (client == null)
            throw new ArgumentNullException(nameof(client));

        _clients[transportName] = client;

        // Set as default if this is the first client
        if (_clients.Count == 1)
        {
            _defaultTransport = transportName;
        }
    }

    /// <summary>
    /// Sets the default transport
    /// </summary>
    public void SetDefaultTransport(string transportName)
    {
        if (!_clients.ContainsKey(transportName))
            throw new ArgumentException($"Transport '{transportName}' is not registered", nameof(transportName));

        _defaultTransport = transportName;
    }
}

/// <summary>
/// Default implementation of ITransportSelector
/// Allows per-request transport selection with fallback logic
/// </summary>
public class DefaultTransportSelector : ITransportSelector
{
    private readonly ITransportClientFactory _factory;
    private string _defaultTransport = "Http";

    public DefaultTransportSelector(ITransportClientFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public string SelectTransport(IRequest request, TransportSelectionContext? context = null)
    {
        context ??= new TransportSelectionContext();

        // Priority 1: Explicitly specified preferred transport
        if (!string.IsNullOrEmpty(context.PreferredTransport))
        {
            var client = _factory.GetClient(context.PreferredTransport);
            if (client != null)
            {
                return context.PreferredTransport;
            }

            if (context.FailIfPreferredUnavailable)
            {
                throw new TransportException($"Preferred transport '{context.PreferredTransport}' is not available");
            }
        }

        // Priority 2: Custom selection logic based on context attributes
        if (context.Attributes.TryGetValue("transport", out var transportObj))
        {
            var transportName = transportObj?.ToString();
            if (!string.IsNullOrEmpty(transportName))
            {
                var client = _factory.GetClient(transportName);
                if (client != null)
                {
                    return transportName;
                }
            }
        }

        // Priority 3: Route-based selection (check if route contains hints)
        var selectedTransport = SelectByRoute(request);
        if (!string.IsNullOrEmpty(selectedTransport))
        {
            return selectedTransport;
        }

        // Priority 4: Request type-based selection
        selectedTransport = SelectByRequestType(request);
        if (!string.IsNullOrEmpty(selectedTransport))
        {
            return selectedTransport;
        }

        // Fallback: Use default transport
        return _defaultTransport;
    }

    private string? SelectByRoute(IRequest request)
    {
        // Example: routes containing /grpc/ use gRPC
        if (request.Route.Contains("/grpc/", StringComparison.OrdinalIgnoreCase))
        {
            return "Grpc";
        }

        return null;
    }

    private string? SelectByRequestType(IRequest request)
    {
        // Can be extended for more sophisticated type-based selection
        return null;
    }

    /// <summary>
    /// Sets the default transport for fallback
    /// </summary>
    public void SetDefaultTransport(string transportName)
    {
        if (_factory.GetClient(transportName) == null)
            throw new ArgumentException($"Transport '{transportName}' is not registered", nameof(transportName));

        _defaultTransport = transportName;
    }
}

/// <summary>
/// Exception thrown by transport operations
/// </summary>
public class TransportException : Exception
{
    public TransportException(string message) : base(message) { }
    public TransportException(string message, Exception innerException) : base(message, innerException) { }
}
