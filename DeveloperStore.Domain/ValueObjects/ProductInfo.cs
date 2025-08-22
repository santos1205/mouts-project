using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Domain.ValueObjects;

/// <summary>
/// External identity for Product from another bounded context.
/// Contains denormalized data to avoid cross-aggregate references.
/// </summary>
public sealed class ProductInfo : IEquatable<ProductInfo>
{
  public Guid ProductId { get; }
  public string Name { get; }
  public string Category { get; }
  public Money UnitPrice { get; }

  // EF Core constructor
  private ProductInfo()
  {
    ProductId = Guid.Empty;
    Name = string.Empty;
    Category = string.Empty;
    UnitPrice = Money.Zero();
  }

  private ProductInfo(Guid productId, string name, string category, Money unitPrice)
  {
    if (productId == Guid.Empty)
      throw new ArgumentException("Product ID cannot be empty", nameof(productId));

    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Product name cannot be null or empty", nameof(name));

    if (string.IsNullOrWhiteSpace(category))
      throw new ArgumentException("Product category cannot be null or empty", nameof(category));

    ProductId = productId;
    Name = name.Trim();
    Category = category.Trim();
    UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
  }

  /// <summary>
  /// Create a ProductInfo instance
  /// </summary>
  public static ProductInfo Of(Guid productId, string name, string category, Money unitPrice)
      => new(productId, name, category, unitPrice);

  public bool Equals(ProductInfo? other)
  {
    return other is not null && ProductId == other.ProductId;
  }

  public override bool Equals(object? obj)
  {
    return Equals(obj as ProductInfo);
  }

  public override int GetHashCode()
  {
    return ProductId.GetHashCode();
  }

  public override string ToString()
  {
    return $"{Name} ({Category}) - {UnitPrice}";
  }

  public static bool operator ==(ProductInfo? left, ProductInfo? right)
  {
    return left?.Equals(right) ?? right is null;
  }

  public static bool operator !=(ProductInfo? left, ProductInfo? right)
  {
    return !(left == right);
  }
}
