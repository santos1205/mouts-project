using DeveloperStore.Domain.Entities;

namespace DeveloperStore.Domain.Repositories;

/// <summary>
/// Repository interface for Sale aggregate root.
/// Following DDD principles, this interface is defined in the Domain layer
/// to maintain dependency inversion - Domain doesn't depend on Infrastructure.
/// </summary>
public interface ISaleRepository
{
  // Query methods
  Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default);
  Task<IReadOnlyList<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<Sale>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

  // Command methods
  Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default);
  Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
  Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default);

  // Persistence
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

  // Existence checks
  Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
  Task<bool> SaleNumberExistsAsync(string saleNumber, CancellationToken cancellationToken = default);

  // Special queries for application services
  Task<IReadOnlyList<object>> GetAllSalesWithCalculatedTotalsAsync(CancellationToken cancellationToken = default);
}
