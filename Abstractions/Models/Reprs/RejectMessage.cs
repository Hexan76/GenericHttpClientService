namespace Corevia.Transporter.Abstractions;
public sealed class RejectMessage : MessageContract
{
    public ICollection<HashtValidation> Validations { get; set; }
    public Exception Exception { get; set; }
}