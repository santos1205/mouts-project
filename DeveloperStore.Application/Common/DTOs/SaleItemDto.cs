namespace DeveloperStore.Application.Common.DTOs;

/// <summary>
/// Data Transfer Object for Sale Item information
/// Represents a line item in a sale
/// </summary>
public class SaleItemDto
{
  public Guid Id { get; set; }
  public Guid ProductId { get; set; }
  public string ProductName { get; set; } = string.Empty;
  public string ProductCategory { get; set; } = string.Empty;
  public decimal ProductUnitPrice { get; set; }
  public string ProductUnitPriceCurrency { get; set; } = "USD";
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
  public string UnitPriceCurrency { get; set; } = "USD";
  public decimal DiscountAmount { get; set; }
  public string DiscountCurrency { get; set; } = "USD";
  public decimal TotalPrice { get; set; }
  public string TotalPriceCurrency { get; set; } = "USD";
  public DateTime CreatedAt { get; set; }
  public DateTime? ModifiedAt { get; set; }
}
