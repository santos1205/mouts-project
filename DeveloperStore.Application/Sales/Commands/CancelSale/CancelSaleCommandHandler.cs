using DeveloperStore.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Application.Sales.Commands.CancelSale;

/// <summary>
/// Handler for the CancelSaleCommand
/// </summary>
public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, bool>
{
  private readonly ISaleRepository _saleRepository;
  private readonly ILogger<CancelSaleCommandHandler> _logger;

  public CancelSaleCommandHandler(ISaleRepository saleRepository, ILogger<CancelSaleCommandHandler> logger)
  {
    _saleRepository = saleRepository;
    _logger = logger;
  }

  public async Task<bool> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Cancelling sale {SaleId} with reason: {Reason}", request.SaleId, request.CancellationReason);

      var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
      if (sale == null)
      {
        _logger.LogWarning("Sale {SaleId} not found", request.SaleId);
        return false;
      }

      if (sale.IsCancelled)
      {
        _logger.LogWarning("Sale {SaleId} is already cancelled", request.SaleId);
        return false;
      }

      sale.Cancel(request.CancellationReason);
      await _saleRepository.UpdateAsync(sale, cancellationToken);
      await _saleRepository.SaveChangesAsync(cancellationToken);

      _logger.LogInformation("Sale {SaleId} cancelled successfully", request.SaleId);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error cancelling sale {SaleId}", request.SaleId);
      throw;
    }
  }
}
