using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Domain.Entities;

/// <summary>
/// Represents a line item within a sale.
/// This is an entity because it has identity and can be tracked individually.
/// </summary>
public class SaleItem : Entity
{
  /// <summary>
  /// Product information (denormalized from Product aggregate)
  /// </summary>
  public ProductInfo Product { get; private set; }

  /// <summary>
  /// Quantity of this product in the sale
  /// </summary>
  public int Quantity { get; private set; }

  /// <summary>
  /// Unit price at the time of sale (may differ from current product price)
  /// </summary>
  public Money UnitPrice { get; private set; }

  /// <summary>
  /// Discount applied to this line item
  /// </summary>
  public Money Discount { get; private set; }

  /// <summary>
  /// Total for this line item (Quantity * UnitPrice - Discount)
  /// </summary>
  public Money LineTotal => (UnitPrice * Quantity) - Discount;

  // EF Core constructor
  private SaleItem()
  {
    // Initialize to satisfy nullable reference types
    Product = null!;
    UnitPrice = null!;
    Discount = null!;
  }

  private SaleItem(ProductInfo product, int quantity, Money? unitPrice = null)
  {
    if (quantity <= 0)
      throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

    if (quantity > 20)
      throw new ArgumentException("Cannot sell more than 20 items of the same product", nameof(quantity));

    Product = product ?? throw new ArgumentNullException(nameof(product));
    Quantity = quantity;
    UnitPrice = unitPrice ?? product.UnitPrice;
    Discount = Money.Zero(UnitPrice.Currency);
  }

  /// <summary>
  /// Create a new sale item
  /// </summary>
  /// <param name="product">Product information</param>
  /// <param name="quantity">Quantity to sell (max 20)</param>
  /// <param name="unitPrice">Override unit price (optional, uses product price if not specified)</param>
  public static SaleItem Create(ProductInfo product, int quantity, Money? unitPrice = null)
  {
    return new SaleItem(product, quantity, unitPrice);
  }

  /// <summary>
  /// Update the quantity of this sale item
  /// </summary>
  /// <param name="newQuantity">New quantity (max 20)</param>
  public void UpdateQuantity(int newQuantity)
  {
    if (newQuantity <= 0)
      throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

    if (newQuantity > 20)
      throw new ArgumentException("Cannot sell more than 20 items of the same product", nameof(newQuantity));

    Quantity = newQuantity;
    MarkAsModified();
  }

  /// <summary>
  /// Apply discount to this sale item
  /// </summary>
  /// <param name="discountAmount">Discount amount</param>
  public void ApplyDiscount(Money discountAmount)
  {
    if (discountAmount.Currency != UnitPrice.Currency)
      throw new InvalidOperationException("Discount currency must match item currency");

    var maxDiscount = UnitPrice * Quantity;
    if (discountAmount.Amount > maxDiscount.Amount)
      throw new ArgumentException("Discount cannot exceed line total", nameof(discountAmount));

    Discount = discountAmount;
    MarkAsModified();
  }

  /// <summary>
  /// Update the unit price of this sale item
  /// </summary>
  /// <param name="newUnitPrice">New unit price</param>
  public void UpdateUnitPrice(Money newUnitPrice)
  {
    if (newUnitPrice.Currency != UnitPrice.Currency)
      throw new InvalidOperationException("New unit price currency must match existing currency");

    UnitPrice = newUnitPrice ?? throw new ArgumentNullException(nameof(newUnitPrice));

    // Reset discount if it exceeds the new line total
    var maxDiscount = UnitPrice * Quantity;
    if (Discount.Amount > maxDiscount.Amount)
    {
      Discount = Money.Zero(UnitPrice.Currency);
    }

    MarkAsModified();
  }
}
