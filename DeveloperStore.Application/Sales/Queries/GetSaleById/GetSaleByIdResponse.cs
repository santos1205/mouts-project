namespace DeveloperStore.Application.Sales.Queries.GetSaleById;

public record GetSaleByIdResponse
{
  public Guid Id { get; init; }
  public string SaleNumber { get; init; } = string.Empty;
  public DateTime SaleDate { get; init; }
  public CustomerDto Customer { get; init; } = null!;
  public BranchDto Branch { get; init; } = null!;
  public List<SaleItemDto> Items { get; init; } = new();
  public int TotalQuantity { get; init; }
  public MoneyDto Subtotal { get; init; } = null!;
  public MoneyDto SaleLevelDiscount { get; init; } = null!;
  public MoneyDto TotalDiscount { get; init; } = null!;
  public MoneyDto TotalAmount { get; init; } = null!;
  public bool IsCancelled { get; init; }
  public string? CancellationReason { get; init; }
  public DateTime CreatedAt { get; init; }
}

public record CustomerDto
{
  public Guid CustomerId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
}

public record BranchDto
{
  public Guid BranchId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Location { get; init; } = string.Empty;
}

public record ProductDto
{
  public Guid ProductId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Category { get; init; } = string.Empty;
  public MoneyDto UnitPrice { get; init; } = null!;
}

public record SaleItemDto
{
  public Guid Id { get; init; }
  public ProductDto Product { get; init; } = null!;
  public int Quantity { get; init; }
  public MoneyDto UnitPrice { get; init; } = null!;
  public MoneyDto Discount { get; init; } = null!;
  public MoneyDto LineTotal { get; init; } = null!;
  public DateTime CreatedAt { get; init; }
}

public record MoneyDto
{
  public decimal Amount { get; init; }
  public string Currency { get; init; } = string.Empty;
}
