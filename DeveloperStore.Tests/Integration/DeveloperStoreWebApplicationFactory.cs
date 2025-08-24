using DeveloperStore.Infrastructure;
using DeveloperStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Tests.Integration;

public class DeveloperStoreWebApplicationFactory : WebApplicationFactory<Program>
{
  private SqliteConnection _connection = null!;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureServices(services =>
    {
      // Remove ALL existing DbContext registrations
      var dbContextDescriptor = services.SingleOrDefault(
              d => d.ServiceType == typeof(DbContextOptions<DeveloperStoreDbContext>));
      if (dbContextDescriptor != null)
        services.Remove(dbContextDescriptor);

      var dbContextServiceDescriptor = services.SingleOrDefault(
              d => d.ServiceType == typeof(DeveloperStoreDbContext));
      if (dbContextServiceDescriptor != null)
        services.Remove(dbContextServiceDescriptor);

      // Remove any other EF Core service registrations that might conflict
      var descriptorsToRemove = services.Where(
              d => d.ServiceType.FullName != null &&
                   (d.ServiceType.FullName.Contains("EntityFramework") ||
                    d.ServiceType == typeof(DbContextOptions)))
              .ToList();

      foreach (var descriptor in descriptorsToRemove)
      {
        services.Remove(descriptor);
      }

      // Create and open SQLite in-memory connection to keep database alive
      _connection = new SqliteConnection("DataSource=:memory:");
      _connection.Open();

      // Add SQLite database for testing (better owned entity support than InMemory)
      services.AddDbContext<DeveloperStoreDbContext>(options =>
          {
            options.UseSqlite(_connection);
            options.EnableSensitiveDataLogging();
          });

      // Build the service provider and create the database
      var serviceProvider = services.BuildServiceProvider();

      using var scope = serviceProvider.CreateScope();
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<DeveloperStoreDbContext>();
      var logger = scopedServices.GetRequiredService<ILogger<DeveloperStoreWebApplicationFactory>>();

      // Ensure the database is created
      db.Database.EnsureCreated();

      try
      {
        // Seed test data if needed
        SeedTestData(db);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the test database");
        throw;
      }
    });

    builder.UseEnvironment("Testing");
  }

  private static void SeedTestData(DeveloperStoreDbContext context)
  {
    // Clear existing data
    context.Sales.RemoveRange(context.Sales);
    context.SaveChanges();

    // Add test data will be handled by individual tests
    // This method can be used for common test data if needed
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _connection?.Dispose();
    }
    base.Dispose(disposing);
  }
}
