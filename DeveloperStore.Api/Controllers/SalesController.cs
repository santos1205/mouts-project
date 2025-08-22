using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Repositories;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

/// <summary>
/// Sales API Controller for testing our persistence layer implementation.
/// This is a basic CRUD controller for demonstration purposes.
/// In Step 4, we'll replace this with proper CQRS implementation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
  private readonly ISaleRepository _saleRepository;
  private readonly ILogger<SalesController> _logger;

  public SalesController(ISaleRepository saleRepository, ILogger<SalesController> logger)
  {
    _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get all sales
  /// </summary>
  [HttpGet]
  public async Task<ActionResult<IEnumerable<object>>> GetSales()
  {
    try
    {
      var sales = await _saleRepository.GetAllAsync();

      var response = sales.Select(s => new
      {
        s.Id,
        s.SaleNumber,
        s.SaleDate,
        Customer = new
        {
          s.Customer.CustomerId,
          s.Customer.Name,
          s.Customer.Email
        },
        Branch = new
        {
          s.Branch.BranchId,
          s.Branch.Name,
          s.Branch.Location
        },
        s.TotalQuantity,
        Subtotal = new { s.Subtotal.Amount, s.Subtotal.Currency },
        TotalDiscount = new { s.TotalDiscount.Amount, s.TotalDiscount.Currency },
        TotalAmount = new { s.TotalAmount.Amount, s.TotalAmount.Currency },
        s.IsCancelled,
        s.CancellationReason,
        ItemCount = s.Items.Count(),
        s.CreatedAt
      });

      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving sales");
      return StatusCode(500, "An error occurred while retrieving sales");
    }
  }

  /// <summary>
  /// Get sale by ID
  /// </summary>
  [HttpGet("{id:guid}")]
  public async Task<ActionResult<object>> GetSale(Guid id)
  {
    try
    {
      var sale = await _saleRepository.GetByIdAsync(id);
      if (sale == null)
      {
        return NotFound($"Sale with ID {id} not found");
      }

      var response = new
      {
        sale.Id,
        sale.SaleNumber,
        sale.SaleDate,
        Customer = new
        {
          sale.Customer.CustomerId,
          sale.Customer.Name,
          sale.Customer.Email
        },
        Branch = new
        {
          sale.Branch.BranchId,
          sale.Branch.Name,
          sale.Branch.Location
        },
        Items = sale.Items.Select(item => new
        {
          item.Id,
          Product = new
          {
            item.Product.ProductId,
            item.Product.Name,
            item.Product.Category,
            UnitPrice = new { item.Product.UnitPrice.Amount, item.Product.UnitPrice.Currency }
          },
          item.Quantity,
          UnitPrice = new { item.UnitPrice.Amount, item.UnitPrice.Currency },
          Discount = new { item.Discount.Amount, item.Discount.Currency },
          LineTotal = new { item.LineTotal.Amount, item.LineTotal.Currency },
          item.CreatedAt
        }),
        sale.TotalQuantity,
        Subtotal = new { sale.Subtotal.Amount, sale.Subtotal.Currency },
        SaleLevelDiscount = new { sale.SaleLevelDiscount.Amount, sale.SaleLevelDiscount.Currency },
        TotalDiscount = new { sale.TotalDiscount.Amount, sale.TotalDiscount.Currency },
        TotalAmount = new { sale.TotalAmount.Amount, sale.TotalAmount.Currency },
        sale.IsCancelled,
        sale.CancellationReason,
        sale.CreatedAt
      };

      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving sale {SaleId}", id);
      return StatusCode(500, "An error occurred while retrieving the sale");
    }
  }

  /// <summary>
  /// Create a new sale for testing
  /// </summary>
  [HttpPost("test-create")]
  public async Task<ActionResult<object>> CreateTestSale()
  {
    try
    {
      // Create test data
      var customer = CustomerInfo.Of(
          customerId: Guid.NewGuid(),
          name: "John Doe",
          email: "john.doe@email.com"
      );

      var branch = BranchInfo.Of(
          branchId: Guid.NewGuid(),
          name: "Main Branch",
          location: "Downtown"
      );

      // Create sale
      var sale = Sale.Create(
          saleNumber: $"SALE-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
          saleDate: DateTime.UtcNow,
          customer: customer,
          branch: branch
      );

      // Add some test items
      var product1 = ProductInfo.Of(
          productId: Guid.NewGuid(),
          name: "Laptop Pro X1",
          category: "Electronics",
          unitPrice: Money.Of(2500m, "BRL")
      );

      var product2 = ProductInfo.Of(
          productId: Guid.NewGuid(),
          name: "Wireless Mouse",
          category: "Accessories",
          unitPrice: Money.Of(150m, "BRL")
      );

      sale.AddItem(product1, 2, Money.Of(2400m, "BRL")); // Discounted price
      sale.AddItem(product2, 3, Money.Of(140m, "BRL"));  // Discounted price

      // Save to database
      await _saleRepository.AddAsync(sale);
      await _saleRepository.SaveChangesAsync();

      _logger.LogInformation("Created test sale {SaleNumber} with ID {SaleId}", sale.SaleNumber, sale.Id);

      // Return created sale
      return CreatedAtAction(
          nameof(GetSale),
          new { id = sale.Id },
          new
          {
            sale.Id,
            sale.SaleNumber,
            Message = "Test sale created successfully",
            ItemsCount = sale.Items.Count,
            TotalAmount = new { sale.TotalAmount.Amount, sale.TotalAmount.Currency }
          }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating test sale");
      return StatusCode(500, "An error occurred while creating the test sale");
    }
  }

  /// <summary>
  /// Get sales by customer ID
  /// </summary>
  [HttpGet("customer/{customerId:guid}")]
  public async Task<ActionResult<IEnumerable<object>>> GetSalesByCustomer(Guid customerId)
  {
    try
    {
      var sales = await _saleRepository.GetByCustomerIdAsync(customerId);

      var response = sales.Select(s => new
      {
        s.Id,
        s.SaleNumber,
        s.SaleDate,
        s.TotalQuantity,
        TotalAmount = new { s.TotalAmount.Amount, s.TotalAmount.Currency },
        s.IsCancelled,
        ItemCount = s.Items.Count()
      });

      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving sales for customer {CustomerId}", customerId);
      return StatusCode(500, "An error occurred while retrieving customer sales");
    }
  }

  /// <summary>
  /// Check if sale number exists
  /// </summary>
  [HttpGet("exists/{saleNumber}")]
  public async Task<ActionResult<object>> CheckSaleNumberExists(string saleNumber)
  {
    try
    {
      var exists = await _saleRepository.SaleNumberExistsAsync(saleNumber);
      return Ok(new { saleNumber, exists });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error checking sale number existence for {SaleNumber}", saleNumber);
      return StatusCode(500, "An error occurred while checking sale number");
    }
  }
}
