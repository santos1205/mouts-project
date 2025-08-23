using AutoMapper;
using DeveloperStore.Application.Common.DTOs;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Repositories;
using DeveloperStore.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CreateSale;

/// <summary>
/// Handler for CreateSaleCommand
/// Implements business logic for creating a new sale following CQRS pattern
/// </summary>
public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleDto>
{
  private readonly ISaleRepository _saleRepository;
  private readonly IValidator<CreateSaleCommand> _validator;
  private readonly IMapper _mapper;

  public CreateSaleCommandHandler(
      ISaleRepository saleRepository,
      IValidator<CreateSaleCommand> validator,
      IMapper mapper)
  {
    _saleRepository = saleRepository;
    _validator = validator;
    _mapper = mapper;
  }

  public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
  {
    // Validate the command
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    // Generate unique sale number
    var saleNumber = await GenerateUniqueSaleNumber();

    // Create value objects
    var customer = CustomerInfo.Of(
        request.CustomerId,
        request.CustomerName,
        request.CustomerEmail
    );

    var branch = BranchInfo.Of(
        request.BranchId,
        request.BranchName,
        request.BranchLocation
    );

    // Create the sale
    var sale = Sale.Create(
        saleNumber,
        DateTime.UtcNow,
        customer,
        branch
    );

    // Add items to the sale
    foreach (var itemDto in request.Items)
    {
      var product = ProductInfo.Of(
          itemDto.ProductId,
          itemDto.ProductName,
          itemDto.ProductCategory,
          Money.Of(itemDto.ProductUnitPrice, itemDto.ProductUnitPriceCurrency)
      );

      var unitPrice = Money.Of(itemDto.UnitPrice, itemDto.UnitPriceCurrency);

      sale.AddItem(product, itemDto.Quantity, unitPrice);
    }

    // Save to repository
    await _saleRepository.AddAsync(sale);
    await _saleRepository.SaveChangesAsync();

    // Map to DTO and return
    return _mapper.Map<SaleDto>(sale);
  }

  private async Task<string> GenerateUniqueSaleNumber()
  {
    const int maxAttempts = 10;

    for (int attempt = 0; attempt < maxAttempts; attempt++)
    {
      var saleNumber = GenerateSaleNumber();

      if (!await _saleRepository.SaleNumberExistsAsync(saleNumber))
      {
        return saleNumber;
      }
    }

    throw new InvalidOperationException("Unable to generate unique sale number after maximum attempts");
  }

  private static string GenerateSaleNumber()
  {
    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    var random = Random.Shared.Next(100, 999);
    return $"S{timestamp}{random}";
  }
}
