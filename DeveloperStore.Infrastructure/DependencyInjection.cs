using DeveloperStore.Domain.Repositories;
using DeveloperStore.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeveloperStore.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services in DI container.
/// This follows the Clean Architecture pattern of keeping infrastructure concerns
/// separate and allowing the API layer to configure dependencies.
/// 
/// Updated to use Raw SQL approach without Entity Framework
/// </summary>
public static class DependencyInjection
{
  /// <summary>
  /// Add infrastructure services using Raw SQL (no Entity Framework)
  /// </summary>
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // Validate connection string
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
      throw new InvalidOperationException("DefaultConnection string is required");
    }

    // Register Raw SQL Repository
    services.AddScoped<ISaleRepository, SaleRepository>();

    return services;
  }

  /// <summary>
  /// Extension method for adding Entity Framework infrastructure services.
  /// Keep this for backward compatibility or future EF integration.
  /// </summary>
  public static IServiceCollection AddInfrastructureWithEntityFramework(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // This method kept for reference - uses Entity Framework
    // Uncomment if you want to switch back to EF
    throw new NotImplementedException("Entity Framework support is disabled. Use AddInfrastructure() instead.");
  }

  /// <summary>
  /// Extension method for adding development-specific infrastructure services.
  /// Raw SQL version - no Entity Framework
  /// </summary>
  public static IServiceCollection AddInfrastructureDevelopment(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // For Raw SQL implementation, development and production are the same
    // You could add development-specific logging or monitoring here
    return services.AddInfrastructure(configuration);
  }
}
