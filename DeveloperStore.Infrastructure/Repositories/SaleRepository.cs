using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Repositories;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DeveloperStore.Infrastructure.Repositories;

/// <summary>
/// Implementation of ISaleRepository using raw SQL queries
/// No Entity Framework - pure SQL approach with PostgreSQL
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly string _connectionString;

    public SaleRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is not configured");
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string salesQuery = @"
            SELECT 
                id, sale_number, sale_date,
                customer_id, customer_name, customer_email,
                branch_id, branch_name, branch_address,
                subtotal_amount, subtotal_currency,
                total_discount_amount, total_discount_currency,
                total_amount, total_amount_currency,
                total_quantity, status, version
            FROM developerstore.sales 
            WHERE id = @id";

        const string itemsQuery = @"
            SELECT 
                id, product_id, product_name, product_category,
                unit_price_amount, unit_price_currency,
                quantity, line_total_amount, line_total_currency
            FROM developerstore.sale_items 
            WHERE sale_id = @saleId 
            ORDER BY created_at";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Get sale data
        using var salesCommand = new NpgsqlCommand(salesQuery, connection);
        salesCommand.Parameters.AddWithValue("@id", id);

        using var salesReader = await salesCommand.ExecuteReaderAsync(cancellationToken);

        if (!await salesReader.ReadAsync(cancellationToken))
            return null;

        // Create sale entity from database row
        var sale = CreateSaleFromReader(salesReader);

        await salesReader.CloseAsync();

        // Get sale items
        using var itemsCommand = new NpgsqlCommand(itemsQuery, connection);
        itemsCommand.Parameters.AddWithValue("@saleId", id);

        using var itemsReader = await itemsCommand.ExecuteReaderAsync(cancellationToken);

        while (await itemsReader.ReadAsync(cancellationToken))
        {
            var saleItem = CreateSaleItemFromReader(itemsReader);
            // Use reflection to add items since AddItem is for business logic
            var itemsField = typeof(Sale).GetField("_items",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var items = (List<SaleItem>)itemsField!.GetValue(sale)!;
            items.Add(saleItem);
        }

        return sale;
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id 
            FROM developerstore.sales 
            WHERE sale_number = @saleNumber";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@saleNumber", saleNumber);

        var id = await command.ExecuteScalarAsync(cancellationToken);

        return id != null ? await GetByIdAsync((Guid)id, cancellationToken) : null;
    }

    public async Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string salesQuery = @"
            SELECT 
                id, sale_number, sale_date,
                customer_id, customer_name, customer_email,
                branch_id, branch_name, branch_address,
                subtotal_amount, subtotal_currency,
                total_discount_amount, total_discount_currency,
                total_amount, total_amount_currency,
                total_quantity, status, version
            FROM developerstore.sales 
            ORDER BY created_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sales = new List<Sale>();

        using var command = new NpgsqlCommand(salesQuery, connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sales.Add(CreateSaleFromReader(reader));
        }

        // For simplicity, we're not loading items for GetAll
        // In a real application, you might want to optimize this
        return sales.AsReadOnly();
    }

    public async Task<IReadOnlyList<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                id, sale_number, sale_date,
                customer_id, customer_name, customer_email,
                branch_id, branch_name, branch_address,
                subtotal_amount, subtotal_currency,
                total_discount_amount, total_discount_currency,
                total_amount, total_amount_currency,
                total_quantity, status, version
            FROM developerstore.sales 
            WHERE customer_id = @customerId
            ORDER BY created_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sales = new List<Sale>();

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sales.Add(CreateSaleFromReader(reader));
        }

        return sales.AsReadOnly();
    }

    public async Task<IReadOnlyList<Sale>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                id, sale_number, sale_date,
                customer_id, customer_name, customer_email,
                branch_id, branch_name, branch_address,
                subtotal_amount, subtotal_currency,
                total_discount_amount, total_discount_currency,
                total_amount, total_amount_currency,
                total_quantity, status, version
            FROM developerstore.sales 
            WHERE branch_id = @branchId
            ORDER BY created_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sales = new List<Sale>();

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@branchId", branchId);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sales.Add(CreateSaleFromReader(reader));
        }

        return sales.AsReadOnly();
    }

    public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                id, sale_number, sale_date,
                customer_id, customer_name, customer_email,
                branch_id, branch_name, branch_address,
                subtotal_amount, subtotal_currency,
                total_discount_amount, total_discount_currency,
                total_amount, total_amount_currency,
                total_quantity, status, version
            FROM developerstore.sales 
            WHERE sale_date >= @startDate AND sale_date <= @endDate
            ORDER BY created_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sales = new List<Sale>();

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@startDate", startDate);
        command.Parameters.AddWithValue("@endDate", endDate);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sales.Add(CreateSaleFromReader(reader));
        }

        return sales.AsReadOnly();
    }

    public async Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        const string insertSaleQuery = @"
            INSERT INTO developerstore.sales (
                id, sale_number, sale_date,
                customer_id, customer_name, customer_email,
                branch_id, branch_name, branch_address,
                subtotal_amount, subtotal_currency,
                total_discount_amount, total_discount_currency,
                total_amount, total_amount_currency,
                total_quantity, status, version
            ) VALUES (
                @id, @sale_number, @sale_date,
                @customer_id, @customer_name, @customer_email,
                @branch_id, @branch_name, @branch_address,
                @subtotal_amount, @subtotal_currency,
                @total_discount_amount, @total_discount_currency,
                @total_amount, @total_amount_currency,
                @total_quantity, @status, @version
            )";

        const string insertItemQuery = @"
            INSERT INTO developerstore.sale_items (
                id, sale_id, product_id, product_name, product_category,
                unit_price_amount, unit_price_currency,
                quantity, line_total_amount, line_total_currency
            ) VALUES (
                @id, @sale_id, @product_id, @product_name, @product_category,
                @unit_price_amount, @unit_price_currency,
                @quantity, @line_total_amount, @line_total_currency
            )";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Insert sale
            using var saleCommand = new NpgsqlCommand(insertSaleQuery, connection, transaction);
            AddSaleParameters(saleCommand, sale);
            await saleCommand.ExecuteNonQueryAsync();

            // Insert sale items
            foreach (var item in sale.Items)
            {
                using var itemCommand = new NpgsqlCommand(insertItemQuery, connection, transaction);
                AddSaleItemParameters(itemCommand, sale.Id, item);
                await itemCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        const string updateSaleQuery = @"
            UPDATE developerstore.sales SET
                sale_date = @sale_date,
                customer_id = @customer_id, customer_name = @customer_name, customer_email = @customer_email,
                branch_id = @branch_id, branch_name = @branch_name, branch_address = @branch_address,
                subtotal_amount = @subtotal_amount, subtotal_currency = @subtotal_currency,
                total_discount_amount = @total_discount_amount, total_discount_currency = @total_discount_currency,
                total_amount = @total_amount, total_amount_currency = @total_amount_currency,
                total_quantity = @total_quantity, status = @status,
                updated_at = CURRENT_TIMESTAMP, version = version + 1
            WHERE id = @id AND version = @version";

        const string deleteItemsQuery = @"DELETE FROM developerstore.sale_items WHERE sale_id = @sale_id";

        const string insertItemQuery = @"
            INSERT INTO developerstore.sale_items (
                id, sale_id, product_id, product_name, product_category,
                unit_price_amount, unit_price_currency,
                quantity, line_total_amount, line_total_currency
            ) VALUES (
                @id, @sale_id, @product_id, @product_name, @product_category,
                @unit_price_amount, @unit_price_currency,
                @quantity, @line_total_amount, @line_total_currency
            )";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update sale
            using var saleCommand = new NpgsqlCommand(updateSaleQuery, connection, transaction);
            AddSaleParameters(saleCommand, sale);

            var rowsAffected = await saleCommand.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
                throw new InvalidOperationException("Sale was modified by another process or does not exist");

            // Delete existing items
            using var deleteCommand = new NpgsqlCommand(deleteItemsQuery, connection, transaction);
            deleteCommand.Parameters.AddWithValue("@sale_id", sale.Id);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            // Insert updated items
            foreach (var item in sale.Items)
            {
                using var itemCommand = new NpgsqlCommand(insertItemQuery, connection, transaction);
                AddSaleItemParameters(itemCommand, sale.Id, item);
                await itemCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        const string query = @"DELETE FROM developerstore.sales WHERE id = @id";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", sale.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In raw SQL implementation, changes are saved immediately
        // This method is kept for interface compliance but doesn't do anything
        return await Task.FromResult(0);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"SELECT EXISTS(SELECT 1 FROM developerstore.sales WHERE id = @id)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)(result ?? false);
    }

    public async Task<bool> SaleNumberExistsAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        const string query = @"SELECT EXISTS(SELECT 1 FROM developerstore.sales WHERE sale_number = @saleNumber)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@saleNumber", saleNumber);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)(result ?? false);
    }

    // Helper methods for mapping between domain entities and database
    private static Sale CreateSaleFromReader(NpgsqlDataReader reader)
    {
        var customer = CustomerInfo.Of(
            reader.GetGuid(reader.GetOrdinal("customer_id")),
            reader.GetString(reader.GetOrdinal("customer_name")),
            reader.GetString(reader.GetOrdinal("customer_email")));

        var branch = BranchInfo.Of(
            reader.GetGuid(reader.GetOrdinal("branch_id")),
            reader.GetString(reader.GetOrdinal("branch_name")),
            reader.GetString(reader.GetOrdinal("branch_address")));

        // Use reflection to create sale with existing data
        var sale = Sale.Create(
            reader.GetString(reader.GetOrdinal("sale_number")),
            reader.GetDateTime(reader.GetOrdinal("sale_date")),
            customer,
            branch);

        // Set the ID using reflection since it's readonly
        var idProperty = typeof(Sale).GetProperty("Id");
        idProperty!.SetValue(sale, reader.GetGuid(reader.GetOrdinal("id")));

        return sale;
    }

    private static SaleItem CreateSaleItemFromReader(NpgsqlDataReader reader)
    {
        var product = ProductInfo.Of(
            reader.GetGuid(reader.GetOrdinal("product_id")),
            reader.GetString(reader.GetOrdinal("product_name")),
            reader.GetString(reader.GetOrdinal("product_category")),
            Money.Of(
                reader.GetDecimal(reader.GetOrdinal("unit_price_amount")),
                reader.GetString(reader.GetOrdinal("unit_price_currency"))));

        // Create sale item using reflection since constructor is internal
        var constructors = typeof(SaleItem).GetConstructors(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var constructor = constructors.First(c => c.GetParameters().Length == 3);

        return (SaleItem)constructor.Invoke(new object[]
        {
            product,
            reader.GetInt32(reader.GetOrdinal("quantity")),
            reader.GetGuid(reader.GetOrdinal("id"))
        });
    }

    private static void AddSaleParameters(NpgsqlCommand command, Sale sale)
    {
        command.Parameters.AddWithValue("@id", sale.Id);
        command.Parameters.AddWithValue("@sale_number", sale.SaleNumber);
        command.Parameters.AddWithValue("@sale_date", sale.SaleDate);

        command.Parameters.AddWithValue("@customer_id", sale.Customer.CustomerId);
        command.Parameters.AddWithValue("@customer_name", sale.Customer.Name);
        command.Parameters.AddWithValue("@customer_email", sale.Customer.Email);

        command.Parameters.AddWithValue("@branch_id", sale.Branch.BranchId);
        command.Parameters.AddWithValue("@branch_name", sale.Branch.Name);
        command.Parameters.AddWithValue("@branch_address", sale.Branch.Location);

        command.Parameters.AddWithValue("@subtotal_amount", sale.Subtotal.Amount);
        command.Parameters.AddWithValue("@subtotal_currency", sale.Subtotal.Currency);

        command.Parameters.AddWithValue("@total_discount_amount", sale.TotalDiscount.Amount);
        command.Parameters.AddWithValue("@total_discount_currency", sale.TotalDiscount.Currency);

        command.Parameters.AddWithValue("@total_amount", sale.TotalAmount.Amount);
        command.Parameters.AddWithValue("@total_amount_currency", sale.TotalAmount.Currency);

        command.Parameters.AddWithValue("@total_quantity", sale.TotalQuantity);
        command.Parameters.AddWithValue("@status", "Active"); // Default status
        command.Parameters.AddWithValue("@version", 1); // For new entities
    }

    private static void AddSaleItemParameters(NpgsqlCommand command, Guid saleId, SaleItem item)
    {
        // Use reflection to get item ID since it might be internal
        var idProperty = typeof(SaleItem).GetProperty("Id");
        var itemId = idProperty?.GetValue(item) ?? Guid.NewGuid();

        command.Parameters.AddWithValue("@id", itemId);
        command.Parameters.AddWithValue("@sale_id", saleId);
        command.Parameters.AddWithValue("@product_id", item.Product.ProductId);
        command.Parameters.AddWithValue("@product_name", item.Product.Name);
        command.Parameters.AddWithValue("@product_category", item.Product.Category);
        command.Parameters.AddWithValue("@unit_price_amount", item.UnitPrice.Amount);
        command.Parameters.AddWithValue("@unit_price_currency", item.UnitPrice.Currency);
        command.Parameters.AddWithValue("@quantity", item.Quantity);
        command.Parameters.AddWithValue("@line_total_amount", item.LineTotal.Amount);
        command.Parameters.AddWithValue("@line_total_currency", item.LineTotal.Currency);
    }
}
