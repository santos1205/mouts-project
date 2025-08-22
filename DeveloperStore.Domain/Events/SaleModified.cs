namespace DeveloperStore.Domain.Events;

/// <summary>
/// Domain event raised when a sale is modified
/// </summary>
public class SaleModified : DomainEvent
{
  public Guid SaleId { get; }
  public string SaleNumber { get; }
  public decimal TotalAmount { get; }
  public string Currency { get; }
  public int ItemCount { get; }
  public string ModificationType { get; }

  public SaleModified(Guid saleId, string saleNumber, decimal totalAmount,
                     string currency, int itemCount, string modificationType)
  {
    SaleId = saleId;
    SaleNumber = saleNumber;
    TotalAmount = totalAmount;
    Currency = currency;
    ItemCount = itemCount;
    ModificationType = modificationType;
  }
}
