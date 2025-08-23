using AutoMapper;
using DeveloperStore.Application.Common.DTOs;
using DeveloperStore.Application.Common.Mappings;
using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Repositories;
using DeveloperStore.Tests.TestUtilities.Builders;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace DeveloperStore.Tests.Unit.Application.Sales.Commands;

/// <summary>
/// Unit tests for CreateSaleCommandHandler
/// Tests the CQRS command handler logic with mocked dependencies
/// </summary>
public class CreateSaleCommandHandlerTests
{
  private readonly Mock<ISaleRepository> _mockRepository;
  private readonly Mock<IValidator<CreateSaleCommand>> _mockValidator;
  private readonly Mock<IMapper> _mockMapper;
  private readonly CreateSaleCommandHandler _handler;

  public CreateSaleCommandHandlerTests()
  {
    _mockRepository = new Mock<ISaleRepository>();
    _mockValidator = new Mock<IValidator<CreateSaleCommand>>();
    _mockMapper = new Mock<IMapper>();

    _handler = new CreateSaleCommandHandler(_mockRepository.Object, _mockValidator.Object, _mockMapper.Object);
  }

  [Fact]
  public async Task Handle_Should_Create_Sale_With_Valid_Command()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    var expectedDto = SaleTestDataBuilder.ValidSaleDto();

    var validationResult = new FluentValidation.Results.ValidationResult();
    _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(validationResult);

    _mockRepository.Setup(x => x.SaleNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

    _mockRepository.Setup(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(It.IsAny<Sale>());

    _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

    _mockMapper.Setup(x => x.Map<SaleDto>(It.IsAny<Sale>()))
              .Returns(expectedDto);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().Be(expectedDto);

    _mockRepository.Verify(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Once);
    _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    _mockMapper.Verify(x => x.Map<SaleDto>(It.IsAny<Sale>()), Times.Once);
  }

  [Fact]
  public async Task Handle_Should_Throw_ValidationException_When_Command_Is_Invalid()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();

    var validationFailure = new FluentValidation.Results.ValidationFailure("CustomerName", "Customer name is required");
    var validationResult = new FluentValidation.Results.ValidationResult(new[] { validationFailure });

    _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(validationResult);

    // Act & Assert
    var action = () => _handler.Handle(command, CancellationToken.None);
    await action.Should().ThrowAsync<ValidationException>();

    _mockRepository.Verify(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_Should_Generate_Unique_Sale_Number()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    var expectedDto = SaleTestDataBuilder.ValidSaleDto();

    var validationResult = new FluentValidation.Results.ValidationResult();
    _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(validationResult);

    // First call returns true (exists), second call returns false (unique)
    _mockRepository.SetupSequence(x => x.SaleNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);

    _mockRepository.Setup(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(It.IsAny<Sale>());

    _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

    _mockMapper.Setup(x => x.Map<SaleDto>(It.IsAny<Sale>()))
              .Returns(expectedDto);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    _mockRepository.Verify(x => x.SaleNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
  }

  [Fact]
  public async Task Handle_Should_Create_Sale_With_Correct_Business_Logic()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    var expectedDto = SaleTestDataBuilder.ValidSaleDto();

    // Ensure we have 5 items to test discount calculation
    command.Items = SaleTestDataBuilder.ValidCreateSaleItemDtos(5);
    foreach (var item in command.Items)
    {
      item.UnitPrice = 10.00m;
      item.UnitPriceCurrency = "USD";
    }

    var validationResult = new FluentValidation.Results.ValidationResult();
    _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(validationResult);

    _mockRepository.Setup(x => x.SaleNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

    _mockRepository.Setup(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(It.IsAny<Sale>());

    _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

    _mockMapper.Setup(x => x.Map<SaleDto>(It.IsAny<Sale>()))
              .Returns(expectedDto);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().Be(expectedDto);

    // Verify repository and mapper were called
    _mockRepository.Verify(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Once);
    _mockMapper.Verify(x => x.Map<SaleDto>(It.IsAny<Sale>()), Times.Once);
  }

  [Fact]
  public async Task Handle_Should_Map_All_Properties_Correctly()
  {
    // Arrange
    var command = SaleTestDataBuilder.ValidCreateSaleCommand();
    var expectedDto = SaleTestDataBuilder.ValidSaleDto();
    command.Items = SaleTestDataBuilder.ValidCreateSaleItemDtos(2);

    var validationResult = new FluentValidation.Results.ValidationResult();
    _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(validationResult);

    _mockRepository.Setup(x => x.SaleNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

    _mockRepository.Setup(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(It.IsAny<Sale>());

    _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

    _mockMapper.Setup(x => x.Map<SaleDto>(It.IsAny<Sale>()))
              .Returns(expectedDto);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().Be(expectedDto);

    _mockMapper.Verify(x => x.Map<SaleDto>(It.IsAny<Sale>()), Times.Once);
  }
}
