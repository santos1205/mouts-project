using DeveloperStore.Application.Sales.Commands.CreateSale;
using FluentValidation;

namespace DeveloperStore.Application.Sales.Commands.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand using FluentValidation
/// Ensures all business rules are met before processing the command
/// </summary>
public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
  public CreateSaleCommandValidator()
  {
    RuleFor(x => x.CustomerId)
        .NotEmpty()
        .WithMessage("Customer ID is required");

    RuleFor(x => x.CustomerName)
        .NotEmpty()
        .WithMessage("Customer name is required")
        .MaximumLength(200)
        .WithMessage("Customer name cannot exceed 200 characters");

    RuleFor(x => x.CustomerEmail)
        .NotEmpty()
        .WithMessage("Customer email is required")
        .EmailAddress()
        .WithMessage("Customer email must be a valid email address")
        .MaximumLength(250)
        .WithMessage("Customer email cannot exceed 250 characters");

    RuleFor(x => x.BranchId)
        .NotEmpty()
        .WithMessage("Branch ID is required");

    RuleFor(x => x.BranchName)
        .NotEmpty()
        .WithMessage("Branch name is required")
        .MaximumLength(200)
        .WithMessage("Branch name cannot exceed 200 characters");

    RuleFor(x => x.BranchLocation)
        .NotEmpty()
        .WithMessage("Branch location is required")
        .MaximumLength(200)
        .WithMessage("Branch location cannot exceed 200 characters");

    RuleFor(x => x.Items)
        .NotEmpty()
        .WithMessage("At least one item is required for a sale")
        .Must(items => items.Count <= 50)
        .WithMessage("Cannot have more than 50 different products in a single sale");

    RuleForEach(x => x.Items).ChildRules(item =>
    {
      item.RuleFor(x => x.ProductId)
              .NotEmpty()
              .WithMessage("Product ID is required");

      item.RuleFor(x => x.ProductName)
              .NotEmpty()
              .WithMessage("Product name is required")
              .MaximumLength(200)
              .WithMessage("Product name cannot exceed 200 characters");

      item.RuleFor(x => x.ProductCategory)
              .NotEmpty()
              .WithMessage("Product category is required")
              .MaximumLength(100)
              .WithMessage("Product category cannot exceed 100 characters");

      item.RuleFor(x => x.Quantity)
              .GreaterThan(0)
              .WithMessage("Quantity must be greater than 0")
              .LessThanOrEqualTo(20)
              .WithMessage("Cannot sell more than 20 items of the same product");

      item.RuleFor(x => x.UnitPrice)
              .GreaterThan(0)
              .WithMessage("Unit price must be greater than 0");

      item.RuleFor(x => x.ProductUnitPrice)
              .GreaterThan(0)
              .WithMessage("Product unit price must be greater than 0");
    });
  }
}
