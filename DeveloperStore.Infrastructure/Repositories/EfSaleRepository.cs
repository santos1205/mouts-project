using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Repositories;
using DeveloperStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Infrastructure.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework with SQLite
/// </summary>
public class EfSaleRepository : ISaleRepository
{
  private readonly DeveloperStoreDbContext _context;

  public EfSaleRepository(DeveloperStoreDbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    // For GetById, we need to load items using navigation property
    return await _context.Sales
        .Include(s => s.Items)
        .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
  }

  public async Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Where(s => s.Customer.CustomerId == customerId)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Where(s => s.Branch.BranchId == branchId)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
  {
    var sales = await _context.Sales
        .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync(cancellationToken);

    return sales.AsReadOnly();
  }

  public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
  {
    return await _context.Sales
        .Include(s => s.Items)
        .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
  }

  public Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    _context.Sales.Add(sale);
    return Task.FromResult(sale);
  }

  public Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    _context.Sales.Update(sale);
    return Task.FromResult(sale);
  }

  public Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    _context.Sales.Remove(sale);
    return Task.CompletedTask;
  }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Sales.AnyAsync(s => s.Id == id, cancellationToken);
  }

  public async Task<bool> SaleNumberExistsAsync(string saleNumber, CancellationToken cancellationToken = default)
  {
    return await _context.Sales.AnyAsync(s => s.SaleNumber == saleNumber, cancellationToken);
  }
}
