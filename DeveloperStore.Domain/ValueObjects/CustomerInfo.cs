namespace DeveloperStore.Domain.ValueObjects;

/// <summary>
/// External identity for Customer from another bounded context.
/// Contains denormalized data to avoid cross-aggregate references.
/// </summary>
public sealed class CustomerInfo : IEquatable<CustomerInfo>
{
  public Guid CustomerId { get; }
  public string Name { get; }
  public string Email { get; }

  // EF Core constructor
  private CustomerInfo()
  {
    CustomerId = Guid.Empty;
    Name = string.Empty;
    Email = string.Empty;
  }

  private CustomerInfo(Guid customerId, string name, string email)
  {
    if (customerId == Guid.Empty)
      throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Customer name cannot be null or empty", nameof(name));

    if (string.IsNullOrWhiteSpace(email))
      throw new ArgumentException("Customer email cannot be null or empty", nameof(email));

    CustomerId = customerId;
    Name = name.Trim();
    Email = email.Trim().ToLowerInvariant();
  }

  /// <summary>
  /// Create a CustomerInfo instance
  /// </summary>
  public static CustomerInfo Of(Guid customerId, string name, string email)
      => new(customerId, name, email);

  public bool Equals(CustomerInfo? other)
  {
    return other is not null && CustomerId == other.CustomerId;
  }

  public override bool Equals(object? obj)
  {
    return Equals(obj as CustomerInfo);
  }

  public override int GetHashCode()
  {
    return CustomerId.GetHashCode();
  }

  public override string ToString()
  {
    return $"{Name} ({Email})";
  }

  public static bool operator ==(CustomerInfo? left, CustomerInfo? right)
  {
    return left?.Equals(right) ?? right is null;
  }

  public static bool operator !=(CustomerInfo? left, CustomerInfo? right)
  {
    return !(left == right);
  }
}
