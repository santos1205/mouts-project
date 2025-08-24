using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DeveloperStore.Api.Controllers;
using DeveloperStore.Api.Models.Requests;
using DeveloperStore.Application.Common.DTOs;
using DeveloperStore.Application.Sales.Queries.GetAllSales;
using DeveloperStore.Application.Sales.Queries.GetSaleById;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Repositories;
using DeveloperStore.Domain.ValueObjects;
using DeveloperStore.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeveloperStore.Tests.Integration.Controllers;

public class SalesControllerIntegrationTests : IClassFixture<DeveloperStoreWebApplicationFactory>
{
  private readonly HttpClient _client;
  private readonly DeveloperStoreWebApplicationFactory _factory;

  public SalesControllerIntegrationTests(DeveloperStoreWebApplicationFactory factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task CreateSale_WithValidRequest_ReturnsCreatedResult()
  {
    // Arrange
    var request = new CreateSaleRequest
    {
      CustomerId = Guid.NewGuid(),
      CustomerName = "John Doe",
      CustomerEmail = "john@example.com",
      BranchId = Guid.NewGuid(),
      BranchName = "Main Branch",
      BranchLocation = "Downtown", // Added missing required field
      Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    ProductCategory = "Electronics",
                    ProductUnitPrice = 10.50m,
                    ProductUnitPriceCurrency = "BRL",
                    Quantity = 2,
                    UnitPrice = 10.50m,
                    UnitPriceCurrency = "BRL"
                }
            }
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/sales", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var locationHeader = response.Headers.Location;
    locationHeader.Should().NotBeNull();
    locationHeader!.ToString().ToLowerInvariant().Should().Contain("/api/sales/");

    var responseContent = await response.Content.ReadAsStringAsync();
    var createdSale = JsonSerializer.Deserialize<SaleDto>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    createdSale.Should().NotBeNull();
    createdSale!.CustomerName.Should().Be("John Doe");
    createdSale.Items.Should().HaveCount(1);
    createdSale.TotalAmount.Should().Be(21.00m); // 2 * 10.50
  }

  [Fact]
  public async Task CreateSale_WithInvalidRequest_ReturnsBadRequest()
  {
    // Arrange
    var request = new CreateSaleRequest
    {
      CustomerId = Guid.NewGuid(),
      CustomerName = "", // Invalid - empty name
      CustomerEmail = "invalid-email", // Invalid email format
      BranchId = Guid.NewGuid(),
      BranchName = "Main Branch",
      Items = new List<CreateSaleItemRequest>()
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/sales", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetSale_WithExistingSaleId_ReturnsOkResult()
  {
    // Arrange
    var saleId = await CreateTestSaleViaApiAsync();

    // Act
    var response = await _client.GetAsync($"/api/sales/{saleId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseContent = await response.Content.ReadAsStringAsync();
    var sale = JsonSerializer.Deserialize<GetSaleByIdResponse>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    sale.Should().NotBeNull();
    sale!.Id.Should().Be(saleId);
    sale.Customer.Name.Should().Be("Test Customer");
  }

  [Fact]
  public async Task GetSale_WithNonExistentSaleId_ReturnsNotFound()
  {
    // Arrange
    var nonExistentId = Guid.NewGuid();

    // Act
    var response = await _client.GetAsync($"/api/sales/{nonExistentId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetAllSales_ReturnsOkResultWithSales()
  {
    // Arrange
    await CreateTestSaleViaApiAsync();
    await CreateTestSaleViaApiAsync();

    // Act
    var response = await _client.GetAsync("/api/sales");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseContent = await response.Content.ReadAsStringAsync();
    var sales = JsonSerializer.Deserialize<List<GetAllSalesResponse>>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    sales.Should().NotBeNull();
    sales!.Should().HaveCountGreaterThanOrEqualTo(2);
  }

  [Fact]
  public async Task CancelSale_WithExistingSaleId_ReturnsNoContent()
  {
    // Arrange
    var saleId = await CreateTestSaleViaApiAsync();

    // Act
    var response = await _client.DeleteAsync($"/api/sales/{saleId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Verify the sale is cancelled
    var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
    var sale = JsonSerializer.Deserialize<GetSaleByIdResponse>(
        await getResponse.Content.ReadAsStringAsync(),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    sale!.IsCancelled.Should().BeTrue();
  }

  [Fact]
  public async Task HealthCheck_ReturnsHealthy()
  {
    // Act
    var response = await _client.GetAsync("/health");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseContent = await response.Content.ReadAsStringAsync();
    responseContent.Should().Contain("Healthy");
  }

  private async Task<Guid> CreateTestSaleViaApiAsync()
  {
    var request = new CreateSaleRequest
    {
      CustomerId = Guid.NewGuid(),
      CustomerName = "Test Customer",
      CustomerEmail = "test@example.com",
      BranchId = Guid.NewGuid(),
      BranchName = "Test Branch",
      BranchLocation = "Downtown",
      Items = new List<CreateSaleItemRequest>
      {
        new CreateSaleItemRequest
        {
          ProductId = Guid.NewGuid(),
          ProductName = "Test Product",
          ProductCategory = "Electronics",
          ProductUnitPrice = 15.00m,
          ProductUnitPriceCurrency = "BRL",
          Quantity = 1,
          UnitPrice = 15.00m,
          UnitPriceCurrency = "BRL"
        }
      }
    };

    var response = await _client.PostAsJsonAsync("/api/sales", request);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var createdSale = JsonSerializer.Deserialize<SaleDto>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    return createdSale!.Id;
  }

  private async Task<Guid> CreateTestSaleAsync()
  {
    using var scope = _factory.Services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<ISaleRepository>();

    var sale = Sale.Create(
        $"SALE-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
        DateTime.UtcNow,
        CustomerInfo.Of(Guid.NewGuid(), "Test Customer", "test@example.com"),
        BranchInfo.Of(Guid.NewGuid(), "Test Branch", "Downtown"));

    var productInfo = ProductInfo.Of(
        Guid.NewGuid(),
        "Test Product",
        "Electronics",
        Money.Of(15.00m));

    sale.AddItem(productInfo, 1);

    await repository.AddAsync(sale);

    return sale.Id;
  }
}
