using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

/// <summary>
/// Debug controller for testing our domain model
/// This is temporary - just for learning debugging
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
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
        "GET /api/debug/create-sample-sale - Test domain rules"
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
}
