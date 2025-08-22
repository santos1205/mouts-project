namespace DeveloperStore.Domain.ValueObjects;

/// <summary>
/// Value object representing money with currency.
/// Immutable and equality is based on value, not identity.
/// </summary>
public sealed class Money : IEquatable<Money>
{
  public decimal Amount { get; }
  public string Currency { get; }

  // EF Core constructor
  private Money()
  {
    Amount = 0;
    Currency = "USD";
  }

  private Money(decimal amount, string currency)
  {
    if (amount < 0)
      throw new ArgumentException("Amount cannot be negative", nameof(amount));

    if (string.IsNullOrWhiteSpace(currency))
      throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

    Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    Currency = currency.ToUpperInvariant();
  }

  /// <summary>
  /// Create a Money instance with the specified amount and currency
  /// </summary>
  public static Money Of(decimal amount, string currency = "USD") => new(amount, currency);

  /// <summary>
  /// Create a zero money instance
  /// </summary>
  public static Money Zero(string currency = "USD") => new(0, currency);

  /// <summary>
  /// Add two money amounts (must be same currency)
  /// </summary>
  public Money Add(Money other)
  {
    if (Currency != other.Currency)
      throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}");

    return new Money(Amount + other.Amount, Currency);
  }

  /// <summary>
  /// Subtract two money amounts (must be same currency)
  /// </summary>
  public Money Subtract(Money other)
  {
    if (Currency != other.Currency)
      throw new InvalidOperationException($"Cannot subtract {other.Currency} from {Currency}");

    return new Money(Amount - other.Amount, Currency);
  }

  /// <summary>
  /// Multiply money by a factor
  /// </summary>
  public Money Multiply(decimal factor)
  {
    if (factor < 0)
      throw new ArgumentException("Factor cannot be negative", nameof(factor));

    return new Money(Amount * factor, Currency);
  }

  /// <summary>
  /// Apply a percentage discount
  /// </summary>
  /// <param name="discountPercentage">Discount as a percentage (e.g., 10 for 10%)</param>
  public Money ApplyDiscount(decimal discountPercentage)
  {
    if (discountPercentage < 0 || discountPercentage > 100)
      throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

    var discountFactor = 1 - (discountPercentage / 100);
    return new Money(Amount * discountFactor, Currency);
  }

  public bool Equals(Money? other)
  {
    return other is not null && Amount == other.Amount && Currency == other.Currency;
  }

  public override bool Equals(object? obj)
  {
    return Equals(obj as Money);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Amount, Currency);
  }

  public override string ToString()
  {
    return $"{Amount:C} {Currency}";
  }

  public static bool operator ==(Money? left, Money? right)
  {
    return left?.Equals(right) ?? right is null;
  }

  public static bool operator !=(Money? left, Money? right)
  {
    return !(left == right);
  }

  public static Money operator +(Money left, Money right)
  {
    return left.Add(right);
  }

  public static Money operator -(Money left, Money right)
  {
    return left.Subtract(right);
  }

  public static Money operator *(Money left, decimal right)
  {
    return left.Multiply(right);
  }
}
