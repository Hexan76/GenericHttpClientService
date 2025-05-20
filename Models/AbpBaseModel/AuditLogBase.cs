namespace HashtApp.Soft.Client.Utilities;

public class AuditLogBase : EntityBase
{
    public string ConcurrencyStamp { get; set; }
    public Dictionary<string,string> ExtraProperties { get; set; }
}