namespace HashtApp.Soft.Client.Utilities;

public interface IEntityBase
{
    Guid Id { get; set; }
}

public interface IEntityBase<TId>
{
    TId Id { get; set; }
}
