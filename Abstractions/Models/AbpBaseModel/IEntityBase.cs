namespace Corevia.Transporter.Abstractions;

public interface IEntityBase
{
    Guid Id { get; set; }
}

public interface IEntityBase<TId>
{
    TId Id { get; set; }
}
