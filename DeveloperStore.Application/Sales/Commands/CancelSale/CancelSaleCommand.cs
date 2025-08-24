using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CancelSale;

/// <summary>
/// Command to cancel a sale
/// </summary>
public class CancelSaleCommand : IRequest<bool>
{
  public Guid SaleId { get; }
  public string CancellationReason { get; }

  public CancelSaleCommand(Guid saleId, string cancellationReason)
  {
    SaleId = saleId;
    CancellationReason = cancellationReason;
  }
}
