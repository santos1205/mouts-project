using DeveloperStore.Application.Common.DTOs;
using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Application.Sales.Queries.GetAllSales;
using DeveloperStore.Application.Sales.Queries.GetSaleById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

/// <summary>
/// Sales API Controller using CQRS pattern with MediatR.
/// Implements proper separation of Commands and Queries for sales operations.
/// Updated from test controller to production CQRS implementation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly ILogger<SalesController> _logger;

  public SalesController(IMediator mediator, ILogger<SalesController> logger)
  {
    _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Get all sales using CQRS query
  /// </summary>
  /// <returns>List of sales with summary information</returns>
  [HttpGet]
  [ProducesResponseType(typeof(List<GetAllSalesResponse>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetAllSalesResponse>>> GetSales()
  {
    try
    {
      _logger.LogInformation("Getting all sales");

      var query = new GetAllSalesQuery();
      var result = await _mediator.Send(query);

      _logger.LogInformation("Retrieved {Count} sales", result.Count);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving sales");
      return StatusCode(StatusCodes.Status500InternalServerError,
        "An error occurred while retrieving sales");
    }
  }

  /// <summary>
  /// Get sale by ID using CQRS query
  /// </summary>
  /// <param name="id">Sale identifier</param>
  /// <returns>Complete sale information including items</returns>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(GetSaleByIdResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetSaleByIdResponse>> GetSale(Guid id)
  {
    try
    {
      _logger.LogInformation("Getting sale {SaleId}", id);

      var query = new GetSaleByIdQuery(id);
      var result = await _mediator.Send(query);

      if (result == null)
      {
        _logger.LogWarning("Sale {SaleId} not found", id);
        return NotFound($"Sale with ID {id} not found");
      }

      _logger.LogInformation("Retrieved sale {SaleId} with {ItemCount} items", id, result.Items.Count);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving sale {SaleId}", id);
      return StatusCode(StatusCodes.Status500InternalServerError,
        "An error occurred while retrieving the sale");
    }
  }

  /// <summary>
  /// Create a new sale using CQRS command
  /// </summary>
  /// <param name="request">Sale creation request</param>
  /// <returns>Created sale information</returns>
  [HttpPost]
  [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<SaleDto>> CreateSale([FromBody] CreateSaleRequest request)
  {
    try
    {
      _logger.LogInformation("Creating sale for customer {CustomerId}", request.CustomerId);

      var command = new CreateSaleCommand
      {
        CustomerId = request.CustomerId,
        CustomerName = request.CustomerName,
        CustomerEmail = request.CustomerEmail,
        BranchId = request.BranchId,
        BranchName = request.BranchName,
        BranchLocation = request.BranchLocation,
        Items = request.Items.Select(item => new CreateSaleItemDto
        {
          ProductId = item.ProductId,
          ProductName = item.ProductName,
          ProductCategory = item.ProductCategory,
          ProductUnitPrice = item.ProductUnitPrice,
          ProductUnitPriceCurrency = item.ProductUnitPriceCurrency,
          Quantity = item.Quantity,
          UnitPrice = item.UnitPrice,
          UnitPriceCurrency = item.UnitPriceCurrency
        }).ToList()
      };

      var result = await _mediator.Send(command);

      _logger.LogInformation("Created sale {SaleNumber} with ID {SaleId}",
        result.SaleNumber, result.Id);

      return CreatedAtAction(
        nameof(GetSale),
        new { id = result.Id },
        result
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating sale");
      return StatusCode(StatusCodes.Status500InternalServerError,
        "An error occurred while creating the sale");
    }
  }

  // TODO: Add additional endpoints as needed:
  // - Update Sale (PUT /api/sales/{id})
  // - Cancel Sale (DELETE /api/sales/{id} or PATCH /api/sales/{id}/cancel)
  // - Get Sales by Customer (GET /api/sales/customer/{customerId})
  // - Advanced filtering and pagination
}
