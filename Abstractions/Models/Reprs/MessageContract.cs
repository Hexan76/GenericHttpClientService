using System.Net;

namespace Corevia.Transporter.Abstractions;
public abstract class MessageContract<TResponseMessage> : MessageContract
where TResponseMessage : class
{
    public TResponseMessage Data { get; set; }
}

public class MessageContract
{
    public string Message { get; set; } = string.Empty;
    public HttpStatusCode Code { get; set; }
    public int Severity { get; set; }
    public MessageType Type { get; set; }
}

public enum MessageType
{
    Information = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}