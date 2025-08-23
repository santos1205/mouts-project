using AutoMapper;
using DeveloperStore.Domain.Repositories;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetAllSales;

public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, List<GetAllSalesResponse>>
{
  private readonly ISaleRepository _saleRepository;
  private readonly IMapper _mapper;

  public GetAllSalesQueryHandler(ISaleRepository saleRepository, IMapper mapper)
  {
    _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
    _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  }

  public async Task<List<GetAllSalesResponse>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
  {
    var sales = await _saleRepository.GetAllAsync();

    return _mapper.Map<List<GetAllSalesResponse>>(sales);
  }
}
