using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetSaleById;

public record GetSaleByIdQuery(Guid SaleId) : IRequest<GetSaleByIdResponse?>;
