namespace DeveloperStore.Domain.Events;

/// <summary>
/// Base class for all domain events.
/// Domain events represent something important that happened in the business domain.
/// </summary>
public abstract class DomainEvent
{
  /// <summary>
  /// Unique identifier for this event instance
  /// </summary>
  public Guid Id { get; } = Guid.NewGuid();

  /// <summary>
  /// When this event occurred
  /// </summary>
  public DateTime OccurredOn { get; } = DateTime.UtcNow;

  /// <summary>
  /// The type of event (used for routing and handling)
  /// </summary>
  public string EventType => GetType().Name;
}
