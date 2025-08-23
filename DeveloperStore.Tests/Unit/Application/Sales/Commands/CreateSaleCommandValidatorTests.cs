using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Tests.TestUtilities.Builders;
using FluentAssertions;

namespace DeveloperStore.Tests.Unit.Application.Sales.Commands;

/// <summary>
/// Unit tests for CreateSaleCommandValidator
/// Tests validation rules for creating sales
/// </summary>
public class CreateSaleCommandValidatorTests
{
  private readonly CreateSaleCommandValidator _validator;

  public CreateSaleCommandValidatorTests()
  {
    _validator = new CreateSaleCommandValidator();
  }

  [Fact]
  public void Should_Pass_Validation_With_Valid_Command()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Should_Fail_Validation_When_CustomerId_Is_Empty()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    command.CustomerId = Guid.Empty;

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName == "CustomerId");
    result.Errors.First(e => e.PropertyName == "CustomerId")
        .ErrorMessage.Should().Be("Customer ID is required");
  }

  [Fact]
  public void Should_Fail_Validation_When_CustomerName_Is_Empty()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    command.CustomerName = string.Empty;

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName == "CustomerName");
  }

  [Fact]
  public void Should_Fail_Validation_When_CustomerEmail_Is_Invalid()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    command.CustomerEmail = "invalid-email";

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName == "CustomerEmail");
    result.Errors.First(e => e.PropertyName == "CustomerEmail")
        .ErrorMessage.Should().Be("Customer email must be a valid email address");
  }

  [Fact]
  public void Should_Fail_Validation_When_Items_Is_Empty()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    command.Items = new List<DeveloperStore.Application.Common.DTOs.CreateSaleItemDto>();

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName == "Items");
  }

  [Fact]
  public void Should_Fail_Validation_When_Item_Quantity_Exceeds_Maximum()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    command.Items.First().Quantity = 25; // Max is 20

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName.Contains("Quantity"));
    result.Errors.First(e => e.PropertyName.Contains("Quantity"))
        .ErrorMessage.Should().Be("Cannot sell more than 20 items of the same product");
  }

  [Fact]
  public void Should_Fail_Validation_When_Item_Quantity_Is_Zero()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    command.Items.First().Quantity = 0;

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName.Contains("Quantity"));
    result.Errors.First(e => e.PropertyName.Contains("Quantity"))
        .ErrorMessage.Should().Be("Quantity must be greater than 0");
  }

  [Fact]
  public void Should_Fail_Validation_When_UnitPrice_Is_Zero()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    command.Items.First().UnitPrice = 0;

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(e => e.PropertyName.Contains("UnitPrice"));
  }
}
