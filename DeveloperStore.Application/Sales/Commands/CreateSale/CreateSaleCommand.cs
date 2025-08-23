using DeveloperStore.Application.Common.DTOs;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CreateSale;

/// <summary>
/// Command to create a new sale
/// Follows CQRS pattern for write operations
/// </summary>
public class CreateSaleCommand : IRequest<SaleDto>
{
  public Guid CustomerId { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public string CustomerEmail { get; set; } = string.Empty;
  public Guid BranchId { get; set; }
  public string BranchName { get; set; } = string.Empty;
  public string BranchLocation { get; set; } = string.Empty;
  public List<CreateSaleItemDto> Items { get; set; } = new();
}
