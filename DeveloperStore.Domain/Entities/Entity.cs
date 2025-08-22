using DeveloperStore.Domain.Events;

namespace DeveloperStore.Domain.Entities;

/// <summary>
/// Base class for all domain entities.
/// Provides identity and domain event handling capabilities.
/// </summary>
public abstract class Entity
{
  private readonly List<DomainEvent> _domainEvents = new();

  /// <summary>
  /// Unique identifier for this entity
  /// </summary>
  public Guid Id { get; protected set; }

  /// <summary>
  /// When this entity was created
  /// </summary>
  public DateTime CreatedAt { get; protected set; }

  /// <summary>
  /// When this entity was last modified
  /// </summary>
  public DateTime? ModifiedAt { get; protected set; }

  /// <summary>
  /// Read-only collection of domain events raised by this entity
  /// </summary>
  public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  protected Entity()
  {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow;
  }

  protected Entity(Guid id)
  {
    Id = id;
    CreatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Add a domain event to be published when this entity is persisted
  /// </summary>
  /// <param name="domainEvent">The domain event to raise</param>
  protected void AddDomainEvent(DomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }

  /// <summary>
  /// Clear all pending domain events (typically called after publishing)
  /// </summary>
  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }

  /// <summary>
  /// Update the modified timestamp (should be called when entity changes)
  /// </summary>
  protected void MarkAsModified()
  {
    ModifiedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Entities are equal if they have the same Id
  /// </summary>
  public override bool Equals(object? obj)
  {
    if (obj is not Entity other)
      return false;

    if (ReferenceEquals(this, other))
      return true;

    return Id == other.Id;
  }

  public override int GetHashCode()
  {
    return Id.GetHashCode();
  }

  public static bool operator ==(Entity? left, Entity? right)
  {
    return left?.Equals(right) ?? right is null;
  }

  public static bool operator !=(Entity? left, Entity? right)
  {
    return !(left == right);
  }
}
