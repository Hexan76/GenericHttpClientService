namespace HashtApp.Soft.Client.Utilities;

public class EntityBase : IEntityBase
{

    public Guid Id { get; set; }
}

public class BaseEntity<TId> : IEntityBase<TId>
{
    TId IEntityBase<TId>.Id { get; set; }
}
