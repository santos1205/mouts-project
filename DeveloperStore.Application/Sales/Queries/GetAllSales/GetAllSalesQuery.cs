using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetAllSales;

public record GetAllSalesQuery : IRequest<List<GetAllSalesResponse>>;
