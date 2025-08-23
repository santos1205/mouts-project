using AutoMapper;
using DeveloperStore.Domain.Repositories;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetSaleById;

public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, GetSaleByIdResponse?>
{
  private readonly ISaleRepository _saleRepository;
  private readonly IMapper _mapper;

  public GetSaleByIdQueryHandler(ISaleRepository saleRepository, IMapper mapper)
  {
    _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
    _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  }

  public async Task<GetSaleByIdResponse?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
  {
    var sale = await _saleRepository.GetByIdAsync(request.SaleId);

    return sale == null ? null : _mapper.Map<GetSaleByIdResponse>(sale);
  }
}
