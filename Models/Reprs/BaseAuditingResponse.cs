namespace HashtApp.Soft.Client.Utilities;
public class BaseAuditingResponse<TKey>
{
    public TKey Id { get; set; }

    public Dictionary<string, object?> ExtraProperties { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }

}