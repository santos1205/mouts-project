namespace DeveloperStore.Application.Common.DTOs;

/// <summary>
/// Data Transfer Object for Sale information
/// Used for API responses and data transfer between layers
/// </summary>
public class SaleDto
{
  public Guid Id { get; set; }
  public string SaleNumber { get; set; } = string.Empty;
  public DateTime SaleDate { get; set; }
  public Guid CustomerId { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public string CustomerEmail { get; set; } = string.Empty;
  public Guid BranchId { get; set; }
  public string BranchName { get; set; } = string.Empty;
  public string BranchLocation { get; set; } = string.Empty;
  public decimal TotalAmount { get; set; }
  public string Currency { get; set; } = "USD";
  public decimal SaleLevelDiscountAmount { get; set; }
  public string SaleLevelDiscountCurrency { get; set; } = "USD";
  public bool IsCancelled { get; set; }
  public string? CancellationReason { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? ModifiedAt { get; set; }
  public List<SaleItemDto> Items { get; set; } = new();
}
