using DeveloperStore.Domain.Entities;
using DeveloperStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// Configure the DbContext with SQLite
var options = new DbContextOptionsBuilder<DeveloperStoreDbContext>()
    .UseSqlite("Data Source=DeveloperStore.Api/developerstore.db")
    .Options;

using var context = new DeveloperStoreDbContext(options);

try
{
  Console.WriteLine("=== Verificando dados na tabela Sales ===\n");

  // Check if database exists and can be connected to
  var canConnect = await context.Database.CanConnectAsync();
  Console.WriteLine($"Conexão com o banco: {(canConnect ? "OK" : "FALHA")}");

  if (!canConnect)
  {
    Console.WriteLine("Não foi possível conectar ao banco de dados.");
    return;
  }

  // Count total sales
  var totalSales = await context.Sales.CountAsync();
  Console.WriteLine($"Total de vendas na tabela Sales: {totalSales}");

  if (totalSales == 0)
  {
    Console.WriteLine("\n❌ A tabela Sales está VAZIA (zerada)");
  }
  else
  {
    Console.WriteLine($"\n✅ A tabela Sales contém {totalSales} registro(s)");

    // Show first few sales
    var recentSales = await context.Sales
        .OrderByDescending(s => s.CreatedAt)
        .Take(5)
        .Select(s => new
        {
          s.Id,
          s.SaleNumber,
          s.SaleDate,
          CustomerName = s.Customer.Name,
          s.IsCancelled,
          s.CreatedAt
        })
        .ToListAsync();

    Console.WriteLine("\nPrimeiras vendas encontradas:");
    foreach (var sale in recentSales)
    {
      Console.WriteLine($"- ID: {sale.Id}");
      Console.WriteLine($"  Número: {sale.SaleNumber}");
      Console.WriteLine($"  Data: {sale.SaleDate:dd/MM/yyyy}");
      Console.WriteLine($"  Cliente: {sale.CustomerName}");
      Console.WriteLine($"  Cancelada: {(sale.IsCancelled ? "Sim" : "Não")}");
      Console.WriteLine($"  Criada em: {sale.CreatedAt:dd/MM/yyyy HH:mm:ss}");
      Console.WriteLine();
    }
  }

  // Check SaleItems table too
  var totalSaleItems = await context.SaleItems.CountAsync();
  Console.WriteLine($"Total de itens na tabela SaleItems: {totalSaleItems}");

  if (totalSaleItems == 0)
  {
    Console.WriteLine("❌ A tabela SaleItems também está VAZIA");
  }
  else
  {
    Console.WriteLine($"✅ A tabela SaleItems contém {totalSaleItems} registro(s)");
  }
}
catch (Exception ex)
{
  Console.WriteLine($"Erro ao verificar os dados: {ex.Message}");
  Console.WriteLine($"Tipo do erro: {ex.GetType().Name}");

  if (ex.InnerException != null)
  {
    Console.WriteLine($"Erro interno: {ex.InnerException.Message}");
  }
}
