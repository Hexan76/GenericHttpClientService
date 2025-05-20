namespace HashtApp.Soft.Client.Utilities;
public sealed class RejectMessage : MessageContract
{
    public ICollection<HashtValidation> Validations { get; set; }
    public Exception Exception { get; set; }
}