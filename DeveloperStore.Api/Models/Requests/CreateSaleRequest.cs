using System.ComponentModel.DataAnnotations;

namespace DeveloperStore.Api.Models.Requests;

/// <summary>
/// Request model for creating a new sale via API
/// </summary>
public class CreateSaleRequest
{
  [Required]
  public Guid CustomerId { get; set; }

  [Required]
  [StringLength(100)]
  public string CustomerName { get; set; } = string.Empty;

  [Required]
  [EmailAddress]
  [StringLength(255)]
  public string CustomerEmail { get; set; } = string.Empty;

  [Required]
  public Guid BranchId { get; set; }

  [Required]
  [StringLength(100)]
  public string BranchName { get; set; } = string.Empty;

  [Required]
  [StringLength(255)]
  public string BranchLocation { get; set; } = string.Empty;

  [Required]
  [MinLength(1)]
  public List<CreateSaleItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Request model for sale items
/// </summary>
public class CreateSaleItemRequest
{
  [Required]
  public Guid ProductId { get; set; }

  [Required]
  [StringLength(100)]
  public string ProductName { get; set; } = string.Empty;

  [Required]
  [StringLength(50)]
  public string ProductCategory { get; set; } = string.Empty;

  [Range(0.01, double.MaxValue)]
  public decimal ProductUnitPrice { get; set; }

  [Required]
  [StringLength(3)]
  public string ProductUnitPriceCurrency { get; set; } = "BRL";

  [Range(1, 20)]
  public int Quantity { get; set; }

  [Range(0.01, double.MaxValue)]
  public decimal UnitPrice { get; set; }

  [Required]
  [StringLength(3)]
  public string UnitPriceCurrency { get; set; } = "BRL";
}
