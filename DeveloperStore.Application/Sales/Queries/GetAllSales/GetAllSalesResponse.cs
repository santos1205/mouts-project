using DeveloperStore.Application.Sales.Queries.GetSaleById;

namespace DeveloperStore.Application.Sales.Queries.GetAllSales;

public record GetAllSalesResponse
{
  public Guid Id { get; init; }
  public string SaleNumber { get; init; } = string.Empty;
  public DateTime SaleDate { get; init; }
  public CustomerDto Customer { get; init; } = null!;
  public BranchDto Branch { get; init; } = null!;
  public int TotalQuantity { get; init; }
  public MoneyDto Subtotal { get; init; } = null!;
  public MoneyDto TotalDiscount { get; init; } = null!;
  public MoneyDto TotalAmount { get; init; } = null!;
  public bool IsCancelled { get; init; }
  public string? CancellationReason { get; init; }
  public int ItemCount { get; init; }
  public DateTime CreatedAt { get; init; }
}
