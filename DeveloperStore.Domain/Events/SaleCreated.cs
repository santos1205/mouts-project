namespace DeveloperStore.Domain.Events;

/// <summary>
/// Domain event raised when a new sale is created
/// </summary>
public class SaleCreated : DomainEvent
{
  public Guid SaleId { get; }
  public string SaleNumber { get; }
  public Guid CustomerId { get; }
  public Guid BranchId { get; }
  public decimal TotalAmount { get; }
  public string Currency { get; }
  public int ItemCount { get; }

  public SaleCreated(Guid saleId, string saleNumber, Guid customerId, Guid branchId,
                    decimal totalAmount, string currency, int itemCount)
  {
    SaleId = saleId;
    SaleNumber = saleNumber;
    CustomerId = customerId;
    BranchId = branchId;
    TotalAmount = totalAmount;
    Currency = currency;
    ItemCount = itemCount;
  }
}
