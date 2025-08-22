namespace DeveloperStore.Domain.Events;

/// <summary>
/// Domain event raised when a sale is cancelled
/// </summary>
public class SaleCancelled : DomainEvent
{
  public Guid SaleId { get; }
  public string SaleNumber { get; }
  public string CancellationReason { get; }
  public decimal RefundAmount { get; }
  public string Currency { get; }

  public SaleCancelled(Guid saleId, string saleNumber, string cancellationReason,
                      decimal refundAmount, string currency)
  {
    SaleId = saleId;
    SaleNumber = saleNumber;
    CancellationReason = cancellationReason;
    RefundAmount = refundAmount;
    Currency = currency;
  }
}
