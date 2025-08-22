using DeveloperStore.Domain.Repositories;
using DeveloperStore.Infrastructure.Persistence;
using DeveloperStore.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeveloperStore.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services in DI container.
/// This follows the Clean Architecture pattern of keeping infrastructure concerns
/// separate and allowing the API layer to configure dependencies.
/// </summary>
public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // PostgreSQL Database Context
    services.AddDbContext<DeveloperStoreDbContext>(options =>
    {
      var connectionString = configuration.GetConnectionString("DefaultConnection");
      options.UseNpgsql(connectionString, npgsqlOptions =>
          {
          npgsqlOptions.MigrationsAssembly(typeof(DeveloperStoreDbContext).Assembly.FullName);
          npgsqlOptions.EnableRetryOnFailure(
                  maxRetryCount: 3,
                  maxRetryDelay: TimeSpan.FromSeconds(5),
                  errorCodesToAdd: null);
        });

      // Enable sensitive data logging in development
      options.EnableSensitiveDataLogging(false); // Set to true only in development
      options.EnableDetailedErrors(false);       // Set to true only in development
    });

    // Repositories
    services.AddScoped<ISaleRepository, SaleRepository>();

    return services;
  }

  /// <summary>
  /// Extension method for adding development-specific infrastructure services.
  /// This includes enabling sensitive data logging and detailed errors for debugging.
  /// </summary>
  public static IServiceCollection AddInfrastructureDevelopment(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddInfrastructure(configuration);

    // Override DbContext options for development
    services.AddDbContext<DeveloperStoreDbContext>(options =>
    {
      var connectionString = configuration.GetConnectionString("DefaultConnection");
      options.UseNpgsql(connectionString, npgsqlOptions =>
          {
          npgsqlOptions.MigrationsAssembly(typeof(DeveloperStoreDbContext).Assembly.FullName);
          npgsqlOptions.EnableRetryOnFailure(
                  maxRetryCount: 3,
                  maxRetryDelay: TimeSpan.FromSeconds(5),
                  errorCodesToAdd: null);
        });

      // Development-specific options
      options.EnableSensitiveDataLogging(true);
      options.EnableDetailedErrors(true);
      options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    });

    return services;
  }
}
