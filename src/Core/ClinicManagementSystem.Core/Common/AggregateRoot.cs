namespace ClinicManagementSystem.Domain.Common;

public abstract class AggregateRoot<T> : BaseEntity<T>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> domainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    protected void ClearDomainEvent() => _domainEvents.Clear();
}

public abstract class AggregateRoot : AggregateRoot<int> { }

