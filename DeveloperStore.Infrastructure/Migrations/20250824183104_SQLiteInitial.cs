using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeveloperStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SQLiteInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SaleNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SaleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    BranchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BranchName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BranchLocation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SaleLevelDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleLevelDiscountCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    IsCancelled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CancellationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProductCategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProductUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductUnitPriceCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    SaleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId",
                table: "SaleItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleNumber",
                table: "Sales",
                column: "SaleNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "Sales");
        }
    }
}
