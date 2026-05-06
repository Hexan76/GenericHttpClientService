namespace Corevia.Transporter.Abstractions;

/// <summary>
/// Specifies how responses should be wrapped/deserialized
/// </summary>
public enum ResponseWrapperProviderType
{
    /// <summary>
    /// Response is wrapped in an AcceptMessage with Data property
    /// </summary>
    AcceptedMessage,

    /// <summary>
    /// Response is wrapped in a MessageContract with metadata
    /// </summary>
    MessageContract,

    /// <summary>
    /// Response is returned as-is without wrapping
    /// </summary>
    Raw
}
