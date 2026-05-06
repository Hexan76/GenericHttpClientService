namespace Corevia.Transporter.Abstractions;

public class AuditLogBase : EntityBase
{
    public string ConcurrencyStamp { get; set; }
    public Dictionary<string,string> ExtraProperties { get; set; }
}