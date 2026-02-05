namespace ApiHub.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class BaseEntity<TKey>
{
    public TKey Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
