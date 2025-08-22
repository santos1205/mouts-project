using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperStore.Infrastructure.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
  public void Configure(EntityTypeBuilder<SaleItem> builder)
  {
    // Table name
    builder.ToTable("SaleItems");

    // Primary key
    builder.HasKey(si => si.Id);
    builder.Property(si => si.Id)
        .ValueGeneratedNever(); // We're using external identity

    // Foreign key to Sale
    builder.Property<Guid>("SaleId")
        .IsRequired();

    // Tracking properties
    builder.Property(si => si.CreatedAt)
        .IsRequired();

    // Product Info - Complex Value Object
    builder.OwnsOne(si => si.Product, productBuilder =>
    {
      productBuilder.Property(p => p.ProductId)
              .HasColumnName("ProductId")
              .IsRequired();

      productBuilder.Property(p => p.Name)
              .HasColumnName("ProductName")
              .IsRequired()
              .HasMaxLength(200);

      productBuilder.Property(p => p.Category)
              .HasColumnName("ProductCategory")
              .IsRequired()
              .HasMaxLength(100);

      // Product's UnitPrice from ProductInfo
      productBuilder.OwnsOne(p => p.UnitPrice, productUnitPriceBuilder =>
          {
          productUnitPriceBuilder.Property(m => m.Amount)
                  .HasColumnName("ProductUnitPrice")
                  .HasColumnType("decimal(18,2)")
                  .IsRequired();

          productUnitPriceBuilder.Property(m => m.Currency)
                  .HasColumnName("ProductUnitPriceCurrency")
                  .IsRequired()
                  .HasMaxLength(3)
                  .HasDefaultValue("BRL");
        });
    });

    // Unit Price - Money Value Object (price at time of sale)
    builder.OwnsOne(si => si.UnitPrice, unitPriceBuilder =>
    {
      unitPriceBuilder.Property(m => m.Amount)
              .HasColumnName("UnitPrice")
              .HasColumnType("decimal(18,2)")
              .IsRequired();

      unitPriceBuilder.Property(m => m.Currency)
              .HasColumnName("UnitPriceCurrency")
              .IsRequired()
              .HasMaxLength(3)
              .HasDefaultValue("BRL");
    });

    // Discount - Money Value Object
    builder.OwnsOne(si => si.Discount, discountBuilder =>
    {
      discountBuilder.Property(m => m.Amount)
              .HasColumnName("DiscountAmount")
              .HasColumnType("decimal(18,2)")
              .IsRequired();

      discountBuilder.Property(m => m.Currency)
              .HasColumnName("DiscountCurrency")
              .IsRequired()
              .HasMaxLength(3)
              .HasDefaultValue("BRL");
    });

    // Quantity
    builder.Property(si => si.Quantity)
        .IsRequired();

    // Index for performance on Sale queries
    builder.HasIndex("SaleId")
        .HasDatabaseName("IX_SaleItems_SaleId");

    // Computed properties - ignore these as they're calculated
    builder.Ignore(si => si.LineTotal);

    // Domain Events - These won't be stored directly in DB
    builder.Ignore(si => si.DomainEvents);
  }
}
