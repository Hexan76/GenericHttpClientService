using System.Net;

namespace HashtApp.Soft.Client.Utilities;
public abstract class MessageContract<TResponseMessage> : MessageContract
where TResponseMessage : class
{
    public TResponseMessage Data { get; set; }
}

public class MessageContract
{
    public string Message { get; set; }
    public HttpStatusCode Code { get; set; }
    public int Severity { get; set; }
    public MessageType Type { get; set; }

}