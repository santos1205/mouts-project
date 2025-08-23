using Bogus;
using DeveloperStore.Application.Common.DTOs;
using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Tests.TestUtilities.Builders;

/// <summary>
/// Test data builder using Bogus for generating realistic test data
/// Provides consistent and valid test data across all tests
/// </summary>
public static class SaleTestDataBuilder
{
  private static readonly Faker Faker = new();

  /// <summary>
  /// Creates a valid CreateSaleCommand with realistic test data
  /// </summary>
  public static CreateSaleCommand ValidCreateSaleCommand()
  {
    return new CreateSaleCommand
    {
      CustomerId = Faker.Random.Guid(),
      CustomerName = Faker.Person.FullName,
      CustomerEmail = Faker.Person.Email,
      BranchId = Faker.Random.Guid(),
      BranchName = Faker.Company.CompanyName(),
      BranchLocation = Faker.Address.City(),
      Items = ValidCreateSaleItemDtos(Faker.Random.Int(1, 5))
    };
  }

  /// <summary>
  /// Creates a list of valid CreateSaleItemDto objects
  /// </summary>
  public static List<CreateSaleItemDto> ValidCreateSaleItemDtos(int count = 3)
  {
    return Enumerable.Range(0, count)
        .Select(_ => ValidCreateSaleItemDto())
        .ToList();
  }

  /// <summary>
  /// Creates a valid CreateSaleItemDto with realistic product data
  /// </summary>
  public static CreateSaleItemDto ValidCreateSaleItemDto()
  {
    var unitPrice = Faker.Random.Decimal(10, 100);
    return new CreateSaleItemDto
    {
      ProductId = Faker.Random.Guid(),
      ProductName = Faker.Commerce.ProductName(),
      ProductCategory = Faker.Commerce.Categories(1).First(),
      ProductUnitPrice = unitPrice,
      ProductUnitPriceCurrency = "USD",
      Quantity = Faker.Random.Int(1, 10),
      UnitPrice = unitPrice,
      UnitPriceCurrency = "USD"
    };
  }

  /// <summary>
  /// Creates a valid Sale domain entity with test data
  /// </summary>
  public static Sale ValidSale()
  {
    var customer = ValidCustomerInfo();
    var branch = ValidBranchInfo();
    var saleNumber = $"S{Faker.Random.Number(1000, 9999)}";
    var saleDate = Faker.Date.Recent(30);

    var sale = Sale.Create(saleNumber, saleDate, customer, branch);

    // Add some items to make it realistic
    var itemCount = Faker.Random.Int(1, 5);
    for (int i = 0; i < itemCount; i++)
    {
      var product = ValidProductInfo();
      var quantity = Faker.Random.Int(1, 10);
      var unitPrice = Money.Of(Faker.Random.Decimal(10, 100), "USD");

      sale.AddItem(product, quantity, unitPrice);
    }

    return sale;
  }

  /// <summary>
  /// Creates CustomerInfo value object with test data
  /// </summary>
  public static CustomerInfo ValidCustomerInfo()
  {
    return CustomerInfo.Of(
        Faker.Random.Guid(),
        Faker.Person.FullName,
        Faker.Person.Email
    );
  }

  /// <summary>
  /// Creates BranchInfo value object with test data
  /// </summary>
  public static BranchInfo ValidBranchInfo()
  {
    return BranchInfo.Of(
        Faker.Random.Guid(),
        Faker.Company.CompanyName(),
        Faker.Address.City()
    );
  }

  /// <summary>
  /// Creates a valid SaleDto with test data
  /// </summary>
  public static SaleDto ValidSaleDto()
  {
    var saleDate = Faker.Date.Recent(30);
    var items = ValidSaleItemDtos(Faker.Random.Int(1, 3));

    return new SaleDto
    {
      Id = Faker.Random.Guid(),
      SaleNumber = $"S{Faker.Random.Number(1000, 9999)}",
      SaleDate = saleDate,
      CustomerId = Faker.Random.Guid(),
      CustomerName = Faker.Person.FullName,
      CustomerEmail = Faker.Person.Email,
      BranchId = Faker.Random.Guid(),
      BranchName = Faker.Company.CompanyName(),
      BranchLocation = Faker.Address.City(),
      Items = items,
      TotalAmount = items.Sum(i => i.TotalPrice),
      Currency = "USD",
      SaleLevelDiscountAmount = 0m,
      SaleLevelDiscountCurrency = "USD",
      IsCancelled = false,
      CreatedAt = saleDate,
      ModifiedAt = saleDate
    };
  }

  /// <summary>
  /// Creates a list of valid SaleItemDto objects
  /// </summary>
  public static List<SaleItemDto> ValidSaleItemDtos(int count = 3)
  {
    return Enumerable.Range(0, count)
        .Select(_ => ValidSaleItemDto())
        .ToList();
  }

  /// <summary>
  /// Creates a valid SaleItemDto with realistic product data
  /// </summary>
  public static SaleItemDto ValidSaleItemDto()
  {
    var unitPrice = Faker.Random.Decimal(10, 100);
    var quantity = Faker.Random.Int(1, 10);
    var saleDate = Faker.Date.Recent(30);

    return new SaleItemDto
    {
      Id = Faker.Random.Guid(),
      ProductId = Faker.Random.Guid(),
      ProductName = Faker.Commerce.ProductName(),
      ProductCategory = Faker.Commerce.Categories(1).First(),
      ProductUnitPrice = unitPrice,
      ProductUnitPriceCurrency = "USD",
      Quantity = quantity,
      UnitPrice = unitPrice,
      UnitPriceCurrency = "USD",
      DiscountAmount = 0m,
      DiscountCurrency = "USD",
      TotalPrice = unitPrice * quantity,
      TotalPriceCurrency = "USD",
      CreatedAt = saleDate,
      ModifiedAt = saleDate
    };
  }

  /// <summary>
  /// Creates ProductInfo value object with test data
  /// </summary>
  public static ProductInfo ValidProductInfo()
  {
    return ProductInfo.Of(
        Faker.Random.Guid(),
        Faker.Commerce.ProductName(),
        Faker.Commerce.Categories(1).First(),
        Money.Of(Faker.Random.Decimal(10, 100), "USD")
    );
  }
}
