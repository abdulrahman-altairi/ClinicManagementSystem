namespace ClinicManagementSystem.Domain.Common;

public abstract class BaseEntity<T>
{
    public T Id { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
}

public abstract class BaseEntity : BaseEntity<int> { }
