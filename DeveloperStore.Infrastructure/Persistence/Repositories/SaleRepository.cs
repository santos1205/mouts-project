using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Repositories;
using DeveloperStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// PostgreSQL implementation of ISaleRepository using Entity Framework Core.
/// This class handles all data access operations for the Sale aggregate.
/// </summary>
public class SaleRepository : ISaleRepository
{
  private readonly DeveloperStoreDbContext _context;

  public SaleRepository(DeveloperStoreDbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Sales
        .Include(s => s.Items) // Load all sale items
        .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
  }

  public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(saleNumber))
      return null;

    return await _context.Sales
        .Include(s => s.Items)
        .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
  }

  public async Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Include(s => s.Items)
        .OrderByDescending(s => s.CreatedAt)  // Changed from SaleDate to CreatedAt for consistency
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Include(s => s.Items)
        .Where(s => s.Customer.CustomerId == customerId)
        .OrderByDescending(s => s.CreatedAt)  // Changed from SaleDate to CreatedAt for consistency
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Include(s => s.Items)
        .Where(s => s.Branch.BranchId == branchId)
        .OrderByDescending(s => s.CreatedAt)  // Changed from SaleDate to CreatedAt for consistency
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
  {
    // Ensure startDate is beginning of day and endDate is end of day
    var normalizedStartDate = startDate.Date;
    var normalizedEndDate = endDate.Date.AddDays(1).AddTicks(-1);

    var sales = await _context.Sales
        .Include(s => s.Items)
        .Where(s => s.SaleDate >= normalizedStartDate && s.SaleDate <= normalizedEndDate)
        .OrderByDescending(s => s.CreatedAt)  // Changed from SaleDate to CreatedAt for consistency
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    if (sale == null)
      throw new ArgumentNullException(nameof(sale));

    var entityEntry = await _context.Sales.AddAsync(sale, cancellationToken);
    return entityEntry.Entity;
  }

  public Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    if (sale == null)
      throw new ArgumentNullException(nameof(sale));

    // EF Core tracks changes automatically
    _context.Sales.Update(sale);
    return Task.FromResult(sale);
  }

  public Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    if (sale == null)
      throw new ArgumentNullException(nameof(sale));

    _context.Sales.Remove(sale);
    return Task.CompletedTask;
  }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Sales
        .AnyAsync(s => s.Id == id, cancellationToken);
  }

  public async Task<bool> SaleNumberExistsAsync(string saleNumber, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(saleNumber))
      return false;

    return await _context.Sales
        .AnyAsync(s => s.SaleNumber == saleNumber, cancellationToken);
  }

  /// <summary>
  /// Get all sales with calculated totals for GetAllSales response.
  /// This method ensures correct money calculations are performed at the database level.
  /// </summary>
  public async Task<IReadOnlyList<object>> GetAllSalesWithCalculatedTotalsAsync(CancellationToken cancellationToken = default)
  {
    // First, get all sales with their items to ensure proper loading
    var sales = await _context.Sales
      .Include(s => s.Items)
      .OrderByDescending(s => s.CreatedAt)
      .ToListAsync(cancellationToken);

    // Then project to anonymous objects with calculated totals
    var salesWithCalculations = sales.Select(s => new
    {
      Id = s.Id,
      SaleNumber = s.SaleNumber,
      SaleDate = s.SaleDate,
      Customer = new
      {
        Id = s.Customer.CustomerId,
        Name = s.Customer.Name,
        Email = s.Customer.Email
      },
      Branch = new
      {
        Id = s.Branch.BranchId,
        Name = s.Branch.Name,
        Location = s.Branch.Location
      },
      // Calculated totals using the same logic as domain entity
      TotalQuantity = s.Items.Sum(i => i.Quantity),
      Subtotal = new
      {
        Amount = s.Items.Sum(i => i.UnitPrice.Amount * i.Quantity),
        Currency = s.Items.FirstOrDefault()?.UnitPrice.Currency ?? "USD"
      },
      TotalDiscount = new
      {
        Amount = s.Items.Sum(i => i.Discount.Amount) + s.SaleLevelDiscount.Amount,
        Currency = s.SaleLevelDiscount.Currency
      },
      TotalAmount = new
      {
        Amount = s.Items.Sum(i => (i.UnitPrice.Amount * i.Quantity) - i.Discount.Amount) - s.SaleLevelDiscount.Amount,
        Currency = s.SaleLevelDiscount.Currency
      },
      IsCancelled = s.IsCancelled,
      CancellationReason = s.CancellationReason,
      ItemCount = s.Items.Count,
      CreatedAt = s.CreatedAt
    }).ToList();

    return salesWithCalculations.Cast<object>().ToList().AsReadOnly();
  }
}
