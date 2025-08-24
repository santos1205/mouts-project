using DeveloperStore.Domain.Events;
using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Domain.Entities;

/// <summary>
/// Sale Aggregate Root - controls all access to sale data and enforces business rules.
/// This is the entry point for all sale-related operations.
/// </summary>
public class Sale : Entity
{
  private readonly List<SaleItem> _items = new();

  /// <summary>
  /// Unique sale number for business identification
  /// </summary>
  public string SaleNumber { get; private set; } = string.Empty;

  /// <summary>
  /// When the sale was made
  /// </summary>
  public DateTime SaleDate { get; private set; }

  /// <summary>
  /// Customer information (denormalized)
  /// </summary>
  public CustomerInfo Customer { get; private set; } = null!;

  /// <summary>
  /// Branch information (denormalized)
  /// </summary>
  public BranchInfo Branch { get; private set; } = null!;

  /// <summary>
  /// Items in this sale (read-only collection)
  /// </summary>
  public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

  /// <summary>
  /// Total quantity of all items
  /// </summary>
  public int TotalQuantity => _items.Sum(i => i.Quantity);

  /// <summary>
  /// Subtotal before discounts
  /// </summary>
  public Money Subtotal => _items.Aggregate(
      Money.Zero(_items.FirstOrDefault()?.UnitPrice.Currency ?? "USD"),
      (sum, item) => sum + (item.UnitPrice * item.Quantity));

  /// <summary>
  /// Total discount applied (item discounts + sale-level discount)
  /// </summary>
  public Money TotalDiscount => _items.Aggregate(
      Money.Zero(_items.FirstOrDefault()?.UnitPrice.Currency ?? "USD"),
      (sum, item) => sum + item.Discount) + SaleLevelDiscount;

  /// <summary>
  /// Sale-level discount (applied based on business rules)
  /// </summary>
  public Money SaleLevelDiscount { get; private set; } = Money.Zero();

  /// <summary>
  /// Final total amount (Subtotal - TotalDiscount)
  /// </summary>
  public Money TotalAmount => Subtotal - TotalDiscount;

  /// <summary>
  /// Whether this sale has been cancelled
  /// </summary>
  public bool IsCancelled { get; private set; }

  /// <summary>
  /// Reason for cancellation (if cancelled)
  /// </summary>
  public string? CancellationReason { get; private set; }

  // EF Core constructor
  private Sale() { }

  private Sale(string saleNumber, DateTime saleDate, CustomerInfo customer, BranchInfo branch)
  {
    if (string.IsNullOrWhiteSpace(saleNumber))
      throw new ArgumentException("Sale number cannot be null or empty", nameof(saleNumber));

    SaleNumber = saleNumber.Trim().ToUpperInvariant();
    SaleDate = saleDate;
    Customer = customer ?? throw new ArgumentNullException(nameof(customer));
    Branch = branch ?? throw new ArgumentNullException(nameof(branch));
    SaleLevelDiscount = Money.Zero();
  }

  /// <summary>
  /// Create a new sale
  /// </summary>
  public static Sale Create(string saleNumber, DateTime saleDate, CustomerInfo customer, BranchInfo branch)
  {
    var sale = new Sale(saleNumber, saleDate, customer, branch);

    // Raise domain event
    sale.AddDomainEvent(new SaleCreated(
        sale.Id,
        sale.SaleNumber,
        sale.Customer.CustomerId,
        sale.Branch.BranchId,
        0, // No items yet
        "USD", // Default currency
        0));

    return sale;
  }

  /// <summary>
  /// Add an item to the sale
  /// </summary>
  public void AddItem(ProductInfo product, int quantity, Money? unitPrice = null)
  {
    if (IsCancelled)
      throw new InvalidOperationException("Cannot modify a cancelled sale");

    // Check if product already exists in sale
    var existingItem = _items.FirstOrDefault(i => i.Product.ProductId == product.ProductId);

    if (existingItem != null)
    {
      // Update existing item quantity (business rule: max 20 per product)
      var newQuantity = existingItem.Quantity + quantity;
      existingItem.UpdateQuantity(newQuantity);
    }
    else
    {
      // Add new item
      var saleItem = SaleItem.Create(product, quantity, unitPrice);
      _items.Add(saleItem);
    }

    // Recalculate sale-level discounts
    ApplyBusinessRules();
    MarkAsModified();

    // Raise domain event
    AddDomainEvent(new SaleModified(
        Id,
        SaleNumber,
        TotalAmount.Amount,
        TotalAmount.Currency,
        TotalQuantity,
        "ItemAdded"));
  }

  /// <summary>
  /// Update an item quantity in the sale
  /// </summary>
  public void UpdateItemQuantity(Guid itemId, int newQuantity)
  {
    if (IsCancelled)
      throw new InvalidOperationException("Cannot modify a cancelled sale");

    var item = _items.FirstOrDefault(i => i.Id == itemId);
    if (item == null)
      throw new ArgumentException("Sale item not found", nameof(itemId));

    item.UpdateQuantity(newQuantity);

    // Recalculate sale-level discounts
    ApplyBusinessRules();
    MarkAsModified();

    // Raise domain event
    AddDomainEvent(new SaleModified(
        Id,
        SaleNumber,
        TotalAmount.Amount,
        TotalAmount.Currency,
        TotalQuantity,
        "ItemQuantityUpdated"));
  }

  /// <summary>
  /// Remove an item from the sale
  /// </summary>
  public void RemoveItem(Guid itemId)
  {
    if (IsCancelled)
      throw new InvalidOperationException("Cannot modify a cancelled sale");

    var item = _items.FirstOrDefault(i => i.Id == itemId);
    if (item == null)
      throw new ArgumentException("Sale item not found", nameof(itemId));

    _items.Remove(item);

    // Recalculate sale-level discounts
    ApplyBusinessRules();
    MarkAsModified();

    // Raise domain event
    AddDomainEvent(new SaleModified(
        Id,
        SaleNumber,
        TotalAmount.Amount,
        TotalAmount.Currency,
        TotalQuantity,
        "ItemRemoved"));
  }

  /// <summary>
  /// Cancel this sale
  /// </summary>
  public void Cancel(string reason)
  {
    if (IsCancelled)
      throw new InvalidOperationException("Sale is already cancelled");

    if (string.IsNullOrWhiteSpace(reason))
      throw new ArgumentException("Cancellation reason is required", nameof(reason));

    var refundAmount = TotalAmount.Amount;

    IsCancelled = true;
    CancellationReason = reason.Trim();
    MarkAsModified();

    // Raise domain event
    AddDomainEvent(new SaleCancelled(
        Id,
        SaleNumber,
        CancellationReason,
        refundAmount,
        TotalAmount.Currency));
  }

  /// <summary>
  /// Apply business rules for discount calculation
  /// </summary>
  private void ApplyBusinessRules()
  {
    if (!_items.Any())
    {
      SaleLevelDiscount = Money.Zero();
      return;
    }

    // Get the currency from the first item
    var currency = _items.First().UnitPrice.Currency;

    // Business Rule: Discount based on individual product quantities
    // < 4 identical items: 0% discount per product
    // 4-9 identical items: 10% discount per product
    // 10-20 identical items: 20% discount per product

    // Apply discount to each item individually based on its quantity
    foreach (var item in _items)
    {
      decimal discountPercentage = item.Quantity switch
      {
        < 4 => 0m,
        >= 4 and <= 9 => 10m,
        >= 10 and <= 20 => 20m,
        _ => 20m // Cap at 20% for more than 20 items (though max is enforced at 20)
      };

      if (discountPercentage > 0)
      {
        var itemTotal = item.UnitPrice * item.Quantity;
        var discountAmount = itemTotal.ApplyDiscount(discountPercentage);
        var itemDiscount = itemTotal - discountAmount;
        item.ApplyDiscount(itemDiscount);
      }
      else
      {
        // No discount for this item
        item.ApplyDiscount(Money.Zero(currency));
      }
    }

    // Sale-level discount is zero since discounts are applied at item level
    SaleLevelDiscount = Money.Zero(currency);
  }
}
