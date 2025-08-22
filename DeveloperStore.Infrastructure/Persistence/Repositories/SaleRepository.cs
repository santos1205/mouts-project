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
        .OrderByDescending(s => s.SaleDate)
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Include(s => s.Items)
        .Where(s => s.Customer.CustomerId == customerId)
        .OrderByDescending(s => s.SaleDate)
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Include(s => s.Items)
        .Where(s => s.Branch.BranchId == branchId)
        .OrderByDescending(s => s.SaleDate)
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
        .OrderByDescending(s => s.SaleDate)
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
}
