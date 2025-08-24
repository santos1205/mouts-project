using System.Text.Json;
using DeveloperStore.Application.Sales.Queries.GetSaleById;
using DeveloperStore.Domain.Repositories;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetAllSales;

public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, List<GetAllSalesResponse>>
{
  private readonly ISaleRepository _saleRepository;

  public GetAllSalesQueryHandler(ISaleRepository saleRepository)
  {
    _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
  }

  public async Task<List<GetAllSalesResponse>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
  {
    // Use the new method that returns calculated totals directly from the database
    var salesData = await _saleRepository.GetAllSalesWithCalculatedTotalsAsync(cancellationToken);

    // Convert anonymous objects to GetAllSalesResponse
    var result = new List<GetAllSalesResponse>();

    foreach (var sale in salesData)
    {
      // Convert the anonymous object to JSON and then deserialize to access properties
      var json = JsonSerializer.Serialize(sale);
      var saleObj = JsonSerializer.Deserialize<JsonElement>(json);

      var response = new GetAllSalesResponse
      {
        Id = saleObj.GetProperty("Id").GetGuid(),
        SaleNumber = saleObj.GetProperty("SaleNumber").GetString() ?? string.Empty,
        SaleDate = saleObj.GetProperty("SaleDate").GetDateTime(),
        Customer = new CustomerDto
        {
          CustomerId = saleObj.GetProperty("Customer").GetProperty("Id").GetGuid(),
          Name = saleObj.GetProperty("Customer").GetProperty("Name").GetString() ?? string.Empty,
          Email = saleObj.GetProperty("Customer").GetProperty("Email").GetString() ?? string.Empty
        },
        Branch = new BranchDto
        {
          BranchId = saleObj.GetProperty("Branch").GetProperty("Id").GetGuid(),
          Name = saleObj.GetProperty("Branch").GetProperty("Name").GetString() ?? string.Empty,
          Location = saleObj.GetProperty("Branch").GetProperty("Location").GetString() ?? string.Empty
        },
        TotalQuantity = saleObj.GetProperty("TotalQuantity").GetInt32(),
        Subtotal = new MoneyDto
        {
          Amount = saleObj.GetProperty("Subtotal").GetProperty("Amount").GetDecimal(),
          Currency = saleObj.GetProperty("Subtotal").GetProperty("Currency").GetString() ?? "USD"
        },
        TotalDiscount = new MoneyDto
        {
          Amount = saleObj.GetProperty("TotalDiscount").GetProperty("Amount").GetDecimal(),
          Currency = saleObj.GetProperty("TotalDiscount").GetProperty("Currency").GetString() ?? "USD"
        },
        TotalAmount = new MoneyDto
        {
          Amount = saleObj.GetProperty("TotalAmount").GetProperty("Amount").GetDecimal(),
          Currency = saleObj.GetProperty("TotalAmount").GetProperty("Currency").GetString() ?? "USD"
        },
        IsCancelled = saleObj.GetProperty("IsCancelled").GetBoolean(),
        CancellationReason = saleObj.TryGetProperty("CancellationReason", out var cancelReason) && cancelReason.ValueKind != JsonValueKind.Null
          ? cancelReason.GetString() : null,
        ItemCount = saleObj.GetProperty("ItemCount").GetInt32(),
        CreatedAt = saleObj.GetProperty("CreatedAt").GetDateTime()
      };

      result.Add(response);
    }

    return result;
  }
}
