namespace Corevia.Transporter.GRPC;

/// <summary>
/// Configuration options for gRPC transport
/// </summary>
public class GrpcClientServiceOptions
{
    /// <summary>
    /// Default gRPC service address
    /// </summary>
    public string DefaultAddress { get; set; } = "https://localhost:50051";

    /// <summary>
    /// Default configuration name for named gRPC clients
    /// </summary>
    public string DefaultClientName { get; set; } = "Default";

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Enable compression for gRPC calls
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Enable TLS/SSL verification
    /// </summary>
    public bool EnableTls { get; set; } = true;
}
