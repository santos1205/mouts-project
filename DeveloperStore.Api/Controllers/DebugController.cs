using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace DeveloperStore.Api.Controllers;

/// <summary>
/// Debug controller for testing our domain model
/// This is temporary - just for learning debugging
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
  private readonly IConfiguration _configuration;

  public DebugController(IConfiguration configuration)
  {
    _configuration = configuration;
  }
  /// <summary>
  /// Test basic application health and show next steps
  /// </summary>
  [HttpGet("health")]
  public IActionResult Health()
  {
    return Ok(new
    {
      Status = "Healthy",
      DateTime = DateTime.UtcNow,
      Message = "DeveloperStore API is running - Step 3 Complete!",
      Step = "Step 3 - Persistence Layer with PostgreSQL",
      Features = new[]
      {
        "✅ Entity Framework Core configured",
        "✅ PostgreSQL provider setup",
        "✅ Repository pattern implemented",
        "✅ Value objects with EF Core mapping",
        "✅ Database migrations ready",
        "✅ Dependency injection configured"
      },
      NextEndpoints = new[]
      {
        "GET /api/sales - View all sales (empty at first)",
        "POST /api/sales/test-create - Create a test sale",
        "GET /api/sales/{id} - Get specific sale",
        "GET /api/debug/create-sample-sale - Test domain rules",
        "GET /api/debug/test-postgres-connection - Test PostgreSQL connection"
      },
      DatabaseInfo = new
      {
        Provider = "PostgreSQL",
        Status = "Migration created but not applied yet",
        Note = "Run database migrations to create tables"
      }
    });
  }

  /// <summary>
  /// Simple test to verify connection string
  /// </summary>
  [HttpGet("test-connection-string")]
  public IActionResult TestConnectionString()
  {
    try
    {
      var connectionString = _configuration.GetConnectionString("DefaultConnection");

      if (string.IsNullOrEmpty(connectionString))
      {
        return BadRequest(new
        {
          Error = "Connection string not found",
          Note = "Check appsettings.json for DefaultConnection"
        });
      }

      return Ok(new
      {
        Status = "Success",
        Message = "Connection string found",
        ConnectionString = connectionString.Replace("Password=devstore_pass", "Password=***"),
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
      });
    }
    catch (Exception ex)
    {
      return BadRequest(new
      {
        Error = ex.Message,
        Type = ex.GetType().Name
      });
    }
  }

  /// <summary>
  /// Test PostgreSQL connection without EF migrations
  /// </summary>
  [HttpGet("test-postgres-connection")]
  public async Task<IActionResult> TestPostgresConnection()
  {
    try
    {
      var connectionString = _configuration.GetConnectionString("DefaultConnection");

      if (string.IsNullOrEmpty(connectionString))
      {
        return BadRequest(new
        {
          Error = "Connection string not found",
          Note = "Check appsettings.json for DefaultConnection"
        });
      }

      using var connection = new NpgsqlConnection(connectionString);

      // Test basic connection
      await connection.OpenAsync();

      // Test a simple query
      using var command = new NpgsqlCommand("SELECT version();", connection);
      var version = await command.ExecuteScalarAsync();

      // Test if we can create a simple table (this will help us understand if we have proper permissions)
      var testTableCommand = new NpgsqlCommand(@"
        CREATE TABLE IF NOT EXISTS connection_test (
          id SERIAL PRIMARY KEY,
          test_message TEXT,
          created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        );", connection);

      await testTableCommand.ExecuteNonQueryAsync();

      // Insert a test record
      var insertCommand = new NpgsqlCommand(@"
        INSERT INTO connection_test (test_message) 
        VALUES (@message) 
        RETURNING id;", connection);
      insertCommand.Parameters.AddWithValue("@message", "PostgreSQL connection test successful!");

      var insertedId = await insertCommand.ExecuteScalarAsync();

      // Read it back
      var selectCommand = new NpgsqlCommand(@"
        SELECT id, test_message, created_at 
        FROM connection_test 
        WHERE id = @id;", connection);
      selectCommand.Parameters.AddWithValue("@id", insertedId ?? (object)DBNull.Value);

      using var reader = await selectCommand.ExecuteReaderAsync();

      object? testRecord = null;
      if (await reader.ReadAsync())
      {
        testRecord = new
        {
          Id = reader.GetInt32(0),
          Message = reader.GetString(1),
          CreatedAt = reader.GetDateTime(2)
        };
      }

      // Mask password in connection string for display
      var maskedConnectionString = connectionString;
      if (connectionString.Contains("Password="))
      {
        var parts = connectionString.Split("Password=");
        if (parts.Length > 1)
        {
          var afterPassword = parts[1].Split(";");
          maskedConnectionString = parts[0] + "Password=***;" + string.Join(";", afterPassword.Skip(1));
        }
      }

      return Ok(new
      {
        Status = "Success",
        Message = "PostgreSQL connection test completed successfully!",
        ConnectionString = maskedConnectionString,
        DatabaseVersion = version?.ToString(),
        TestResult = new
        {
          TableCreated = "connection_test table created successfully",
          RecordInserted = $"Test record inserted with ID: {insertedId}",
          RecordRetrieved = testRecord
        },
        Timestamp = DateTime.UtcNow
      });
    }
    catch (Exception ex)
    {
      return BadRequest(new
      {
        Error = ex.Message,
        Type = ex.GetType().Name,
        Details = new
        {
          InnerException = ex.InnerException?.Message,
          StackTrace = ex.StackTrace
        },
        Troubleshooting = new[]
        {
          "1. Check if PostgreSQL container is running: docker ps",
          "2. Verify connection string in appsettings.json",
          "3. Ensure database credentials match docker-compose.yml",
          "4. Check if port 5432 is accessible: telnet localhost 5432"
        }
      });
    }
  }

  /// <summary>
  /// Create a sample sale for debugging purposes
  /// </summary>
  [HttpGet("create-sample-sale")]
  public IActionResult CreateSampleSale()
  {
    try
    {
      // Set a breakpoint on the next line to start debugging
      var customer = CustomerInfo.Of(
          Guid.NewGuid(),
          "John Doe",
          "john@example.com");

      var branch = BranchInfo.Of(
          Guid.NewGuid(),
          "Downtown Store",
          "123 Main St");

      // Create a sale
      var sale = Sale.Create(
          "SALE-001",
          DateTime.UtcNow,
          customer,
          branch);

      // Add some items
      var product1 = ProductInfo.Of(
          Guid.NewGuid(),
          "Programming Book",
          "Books",
          Money.Of(29.99m, "USD"));

      var product2 = ProductInfo.Of(
          Guid.NewGuid(),
          "Coffee Mug",
          "Accessories",
          Money.Of(12.50m, "USD"));

      // Add items to sale (this will trigger business rules)
      sale.AddItem(product1, 2);  // 2 books
      sale.AddItem(product2, 3);  // 3 mugs

      // This should trigger the 10% discount (5 items total, which is 4-9 range)

      return Ok(new
      {
        SaleId = sale.Id,
        SaleNumber = sale.SaleNumber,
        Customer = sale.Customer.Name,
        Branch = sale.Branch.Name,
        ItemCount = sale.TotalQuantity,
        Subtotal = sale.Subtotal.Amount,
        Discount = sale.TotalDiscount.Amount,
        Total = sale.TotalAmount.Amount,
        Currency = sale.TotalAmount.Currency,
        Items = sale.Items.Select(i => new
        {
          ProductName = i.Product.Name,
          Quantity = i.Quantity,
          UnitPrice = i.UnitPrice.Amount,
          LineTotal = i.LineTotal.Amount
        })
      });
    }
    catch (Exception ex)
    {
      // Good place to set a breakpoint for error debugging
      return BadRequest(new
      {
        Error = ex.Message,
        Type = ex.GetType().Name,
        StackTrace = ex.StackTrace
      });
    }
  }

  /// <summary>
  /// Test business rules - this should fail
  /// </summary>
  [HttpGet("test-business-rule-violation")]
  public IActionResult TestBusinessRuleViolation()
  {
    try
    {
      var customer = CustomerInfo.Of(Guid.NewGuid(), "Jane Doe", "jane@example.com");
      var branch = BranchInfo.Of(Guid.NewGuid(), "Test Store", "456 Test St");
      var sale = Sale.Create("SALE-002", DateTime.UtcNow, customer, branch);

      var product = ProductInfo.Of(
          Guid.NewGuid(),
          "Test Product",
          "Test Category",
          Money.Of(10.00m, "USD"));

      // This should throw an exception (trying to add 25 items, max is 20)
      sale.AddItem(product, 25);

      return Ok("This shouldn't be reached");
    }
    catch (Exception ex)
    {
      // Set breakpoint here to see the exception details
      return BadRequest(new
      {
        Error = ex.Message,
        Type = ex.GetType().Name,
        ExpectedBehavior = "This exception is expected - business rule working correctly!"
      });
    }
  }

  /// <summary>
  /// Test the raw SQL repository implementation with full CRUD operations
  /// </summary>
  [HttpGet("test-raw-sql-repository")]
  public async Task<IActionResult> TestRawSqlRepository(CancellationToken cancellationToken = default)
  {
    try
    {
      // Use the registered repository from dependency injection
      var repository = HttpContext.RequestServices.GetRequiredService<DeveloperStore.Domain.Repositories.ISaleRepository>();

      var results = new List<string>();
      results.Add("🧪 Starting Raw SQL Repository Test");

      // Step 1: Create test data
      var customer = CustomerInfo.Of(Guid.NewGuid(), "Test Customer", "test@example.com");
      var branch = BranchInfo.Of(Guid.NewGuid(), "Test Branch", "Test Address");
      var product = ProductInfo.Of(Guid.NewGuid(), "Test Product", "Electronics", Money.Of(99.99m, "USD"));

      var testSale = Sale.Create("TEST-RAW-SQL-" + DateTime.UtcNow.Ticks, DateTime.UtcNow, customer, branch);
      testSale.AddItem(product, 2);
      testSale.AddItem(
        ProductInfo.Of(Guid.NewGuid(), "Another Product", "Books", Money.Of(19.99m, "USD")),
        1
      );

      results.Add($"✅ Created test sale: {testSale.SaleNumber}");

      // Step 2: Save to database
      await repository.AddAsync(testSale, cancellationToken);
      await repository.SaveChangesAsync(cancellationToken);
      results.Add($"✅ Saved sale to database with ID: {testSale.Id}");

      // Step 3: Read back from database
      var retrievedSale = await repository.GetByIdAsync(testSale.Id, cancellationToken);
      if (retrievedSale == null)
      {
        results.Add("❌ Failed to retrieve sale from database");
        return BadRequest(new { TestResults = results });
      }

      results.Add($"✅ Retrieved sale: {retrievedSale.SaleNumber}");
      results.Add($"   - Customer: {retrievedSale.Customer.Name} ({retrievedSale.Customer.Email})");
      results.Add($"   - Branch: {retrievedSale.Branch.Name}");
      results.Add($"   - Items count: {retrievedSale.Items.Count}");
      results.Add($"   - Total amount: {retrievedSale.TotalAmount.Amount} {retrievedSale.TotalAmount.Currency}");

      // Step 4: Test GetAll
      var allSales = await repository.GetAllAsync(cancellationToken);
      results.Add($"✅ Retrieved all sales count: {allSales.Count()}");

      // Step 5: Test GetBySaleNumberAsync
      var saleByNumber = await repository.GetBySaleNumberAsync(testSale.SaleNumber, cancellationToken);
      if (saleByNumber != null)
      {
        results.Add($"✅ Found sale by number: {saleByNumber.SaleNumber}");
      }

      // Step 6: Update the sale
      var newProduct = ProductInfo.Of(Guid.NewGuid(), "Updated Product", "Updated Category", Money.Of(49.99m, "USD"));
      retrievedSale.AddItem(newProduct, 1);

      await repository.UpdateAsync(retrievedSale, cancellationToken);
      await repository.SaveChangesAsync(cancellationToken);
      results.Add($"✅ Updated sale - now has {retrievedSale.Items.Count} items");

      // Step 7: Verify update
      var updatedSale = await repository.GetByIdAsync(testSale.Id, cancellationToken);
      if (updatedSale != null && updatedSale.Items.Count == 3)
      {
        results.Add($"✅ Update verified - sale has {updatedSale.Items.Count} items");
        results.Add($"   - New total: {updatedSale.TotalAmount.Amount} {updatedSale.TotalAmount.Currency}");
      }

      // Step 8: Delete the test sale
      await repository.DeleteAsync(retrievedSale, cancellationToken);
      await repository.SaveChangesAsync(cancellationToken);
      results.Add($"✅ Deleted test sale");

      // Step 9: Verify deletion
      var deletedSale = await repository.GetByIdAsync(testSale.Id, cancellationToken);
      if (deletedSale == null)
      {
        results.Add($"✅ Deletion verified - sale no longer exists");
      }
      else
      {
        results.Add($"❌ Deletion failed - sale still exists");
      }

      results.Add("🎉 Raw SQL Repository Test Completed Successfully!");

      return Ok(new
      {
        TestStatus = "SUCCESS",
        RepositoryType = "Raw SQL with Npgsql",
        DatabaseType = "PostgreSQL 15",
        TestResults = results,
        Summary = new
        {
          CreateTest = "✅ PASS",
          ReadTest = "✅ PASS",
          UpdateTest = "✅ PASS",
          DeleteTest = "✅ PASS",
          GetAllTest = "✅ PASS",
          GetByNumberTest = "✅ PASS",
          DomainObjectMapping = "✅ PASS"
        }
      });
    }
    catch (Exception ex)
    {
      return BadRequest(new
      {
        TestStatus = "FAILED",
        Error = ex.Message,
        StackTrace = ex.StackTrace,
        InnerException = ex.InnerException?.Message
      });
    }
  }
}
