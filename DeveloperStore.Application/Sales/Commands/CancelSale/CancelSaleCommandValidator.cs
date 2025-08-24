using FluentValidation;

namespace DeveloperStore.Application.Sales.Commands.CancelSale;

/// <summary>
/// Validator for the CancelSaleCommand
/// </summary>
public class CancelSaleCommandValidator : AbstractValidator<CancelSaleCommand>
{
  public CancelSaleCommandValidator()
  {
    RuleFor(x => x.SaleId)
      .NotEmpty()
      .WithMessage("Sale ID is required");

    RuleFor(x => x.CancellationReason)
      .NotEmpty()
      .WithMessage("Cancellation reason is required")
      .MaximumLength(500)
      .WithMessage("Cancellation reason cannot exceed 500 characters");
  }
}
