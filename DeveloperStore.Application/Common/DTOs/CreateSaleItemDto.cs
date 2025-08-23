namespace DeveloperStore.Application.Common.DTOs;

/// <summary>
/// Input DTO for creating a new sale item
/// Used in CreateSale commands
/// </summary>
public class CreateSaleItemDto
{
  public Guid ProductId { get; set; }
  public string ProductName { get; set; } = string.Empty;
  public string ProductCategory { get; set; } = string.Empty;
  public decimal ProductUnitPrice { get; set; }
  public string ProductUnitPriceCurrency { get; set; } = "USD";
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
  public string UnitPriceCurrency { get; set; } = "USD";
}
