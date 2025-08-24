using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using DeveloperStore.Tests.TestUtilities.Builders;
using FluentAssertions;

namespace DeveloperStore.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Sale aggregate root
/// Tests business logic and domain rules in isolation
/// </summary>
public class SaleTests
{
  [Fact]
  public void Sale_Should_Be_Created_With_Valid_Data()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();
    var saleNumber = "S001";
    var saleDate = DateTime.UtcNow;

    // Act
    var sale = Sale.Create(saleNumber, saleDate, customer, branch);

    // Assert
    sale.Should().NotBeNull();
    sale.Id.Should().NotBeEmpty();
    sale.SaleNumber.Should().Be(saleNumber.ToUpperInvariant());
    sale.Customer.CustomerId.Should().Be(customer.CustomerId);
    sale.Branch.BranchId.Should().Be(branch.BranchId);
    sale.SaleDate.Should().Be(saleDate);
    sale.Items.Should().BeEmpty();
    sale.TotalAmount.Amount.Should().Be(0);
    sale.IsCancelled.Should().BeFalse();
  }

  [Fact]
  public void Sale_Should_Calculate_Discount_Correctly_For_Less_Than_4_Items()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();
    var sale = Sale.Create("S001", DateTime.UtcNow, customer, branch);
    var product = SaleTestDataBuilder.ValidProductInfo();
    var unitPrice = Money.Of(10.00m, "USD");

    // Act - Add 3 items (should have 0% discount)
    sale.AddItem(product, 3, unitPrice);

    // Assert
    sale.SaleLevelDiscount.Amount.Should().Be(0.00m);
    sale.TotalAmount.Amount.Should().Be(30.00m); // 3 * 10.00, no discount
  }

  [Fact]
  public void Sale_Should_Calculate_Discount_Correctly_For_4_To_9_Items()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();
    var sale = Sale.Create("S002", DateTime.UtcNow, customer, branch);
    var product = SaleTestDataBuilder.ValidProductInfo();
    var unitPrice = Money.Of(10.00m, "USD");

    // Act - Add 5 items (should trigger 10% discount on this product)
    sale.AddItem(product, 5, unitPrice);

    // Assert
    sale.SaleLevelDiscount.Amount.Should().Be(0m); // Discounts are applied at item level
    sale.TotalAmount.Amount.Should().Be(45.00m); // 50.00 - 5.00 (item discount)
  }

  [Fact]
  public void Sale_Should_Calculate_Discount_Correctly_For_10_To_20_Items()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();
    var sale = Sale.Create("S003", DateTime.UtcNow, customer, branch);
    var product = SaleTestDataBuilder.ValidProductInfo();
    var unitPrice = Money.Of(10.00m, "USD");

    // Act - Add 15 items (should trigger 20% discount on this product)
    sale.AddItem(product, 15, unitPrice);

    // Assert
    sale.SaleLevelDiscount.Amount.Should().Be(0m); // Discounts are applied at item level
    sale.TotalAmount.Amount.Should().Be(120.00m); // 150.00 - 30.00 (item discount)
  }

  [Fact]
  public void Sale_Should_Throw_Exception_When_Quantity_Exceeds_Maximum()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();
    var sale = Sale.Create("S004", DateTime.UtcNow, customer, branch);
    var product = SaleTestDataBuilder.ValidProductInfo();
    var unitPrice = Money.Of(10.00m, "USD");

    // Act & Assert
    var action = () => sale.AddItem(product, 25, unitPrice); // Max is 20
    action.Should().Throw<ArgumentException>()
        .WithMessage("*Cannot sell more than 20 items*");
  }

  [Fact]
  public void Sale_Should_Add_Multiple_Different_Products()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();
    var sale = Sale.Create("S005", DateTime.UtcNow, customer, branch);
    var product1 = SaleTestDataBuilder.ValidProductInfo();
    var product2 = SaleTestDataBuilder.ValidProductInfo();
    var unitPrice1 = Money.Of(15.00m, "USD");
    var unitPrice2 = Money.Of(25.00m, "USD");

    // Act
    sale.AddItem(product1, 2, unitPrice1); // 2 * 15.00 = 30.00 (no discount - less than 4)
    sale.AddItem(product2, 3, unitPrice2); // 3 * 25.00 = 75.00 (no discount - less than 4)

    // Assert
    sale.Items.Should().HaveCount(2);
    sale.TotalQuantity.Should().Be(5); // 2 + 3
    sale.SaleLevelDiscount.Amount.Should().Be(0m); // No discount - per-product quantities are less than 4
    sale.TotalAmount.Amount.Should().Be(105.00m); // 30.00 + 75.00
  }

  [Fact]
  public void Sale_Should_Be_Cancellable_With_Reason()
  {
    // Arrange
    var sale = SaleTestDataBuilder.ValidSale();
    var cancellationReason = "Customer requested cancellation";

    // Act
    sale.Cancel(cancellationReason);

    // Assert
    sale.IsCancelled.Should().BeTrue();
    sale.CancellationReason.Should().Be(cancellationReason);
  }

  [Fact]
  public void Sale_Should_Throw_Exception_When_Cancelling_Already_Cancelled_Sale()
  {
    // Arrange
    var sale = SaleTestDataBuilder.ValidSale();
    sale.Cancel("First cancellation");

    // Act & Assert
    var action = () => sale.Cancel("Second cancellation");
    action.Should().Throw<InvalidOperationException>()
        .WithMessage("*already cancelled*");
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Sale_Should_Throw_Exception_For_Invalid_Sale_Number(string invalidSaleNumber)
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();

    // Act & Assert
    var action = () => Sale.Create(invalidSaleNumber, DateTime.UtcNow, customer, branch);
    action.Should().Throw<ArgumentException>()
        .WithMessage("*Sale number cannot be null or empty*");
  }

  [Fact]
  public void Sale_Should_Throw_Exception_For_Null_Sale_Number()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();

    // Act & Assert
    var action = () => Sale.Create(null!, DateTime.UtcNow, customer, branch);
    action.Should().Throw<ArgumentException>()
        .WithMessage("*Sale number cannot be null or empty*");
  }

  [Fact]
  public void Sale_Should_Update_ModifiedAt_When_Items_Are_Added()
  {
    // Arrange
    var customer = SaleTestDataBuilder.ValidCustomerInfo();
    var branch = SaleTestDataBuilder.ValidBranchInfo();
    var sale = Sale.Create("S006", DateTime.UtcNow, customer, branch);
    var product = SaleTestDataBuilder.ValidProductInfo();
    var originalModifiedAt = sale.ModifiedAt;

    // Act
    Thread.Sleep(10); // Small delay to ensure time difference
    sale.AddItem(product, 2, Money.Of(10.00m, "USD"));

    // Assert
    sale.ModifiedAt.Should().NotBe(originalModifiedAt);
    sale.ModifiedAt.Should().BeAfter(sale.CreatedAt);
  }
}
