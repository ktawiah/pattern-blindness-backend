namespace PatternBlindness.Domain.Common;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract record DomainEvent
{
  public Guid Id { get; } = Guid.NewGuid();
  public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
