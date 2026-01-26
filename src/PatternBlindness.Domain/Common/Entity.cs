namespace PatternBlindness.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides identity and domain event support.
/// </summary>
public abstract class Entity
{
  private readonly List<DomainEvent> _domainEvents = [];

  public Guid Id { get; protected set; }
  public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; protected set; }

  public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  protected void AddDomainEvent(DomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }

  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }

  public override bool Equals(object? obj)
  {
    if (obj is not Entity other)
      return false;

    if (ReferenceEquals(this, other))
      return true;

    if (GetType() != other.GetType())
      return false;

    if (Id == Guid.Empty || other.Id == Guid.Empty)
      return false;

    return Id == other.Id;
  }

  public override int GetHashCode()
  {
    return (GetType().ToString() + Id).GetHashCode();
  }

  public static bool operator ==(Entity? a, Entity? b)
  {
    if (a is null && b is null)
      return true;

    if (a is null || b is null)
      return false;

    return a.Equals(b);
  }

  public static bool operator !=(Entity? a, Entity? b)
  {
    return !(a == b);
  }
}
