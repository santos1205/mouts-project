using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperStore.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        // Table name
        builder.ToTable("Sales");

        // Primary key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedNever(); // We're using external identity

        // Tracking properties
        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // Sale number - unique business identifier
        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.SaleNumber)
            .IsUnique()
            .HasDatabaseName("IX_Sales_SaleNumber");

        // Sale date
        builder.Property(s => s.SaleDate)
            .IsRequired();

        // Cancellation properties
        builder.Property(s => s.IsCancelled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.CancellationReason)
            .IsRequired(false)
            .HasMaxLength(500);

        // Customer Info - Complex Value Object
        builder.OwnsOne(s => s.Customer, customerBuilder =>
        {
            customerBuilder.Property(c => c.CustomerId)
                .HasColumnName("CustomerId")
                .IsRequired();

            customerBuilder.Property(c => c.Name)
                .HasColumnName("CustomerName")
                .IsRequired()
                .HasMaxLength(200);

            customerBuilder.Property(c => c.Email)
                .HasColumnName("CustomerEmail")
                .IsRequired()
                .HasMaxLength(250);
        });

        // Branch Info - Complex Value Object
        builder.OwnsOne(s => s.Branch, branchBuilder =>
        {
            branchBuilder.Property(b => b.BranchId)
                .HasColumnName("BranchId")
                .IsRequired();

            branchBuilder.Property(b => b.Name)
                .HasColumnName("BranchName")
                .IsRequired()
                .HasMaxLength(200);

            branchBuilder.Property(b => b.Location)
                .HasColumnName("BranchLocation")
                .IsRequired()
                .HasMaxLength(200);
        });

        // Sale Level Discount - Money Value Object
        builder.OwnsOne(s => s.SaleLevelDiscount, discountBuilder =>
        {
            discountBuilder.Property(m => m.Amount)
                .HasColumnName("SaleLevelDiscountAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            discountBuilder.Property(m => m.Currency)
                .HasColumnName("SaleLevelDiscountCurrency")
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        // Navigation to SaleItems (One-to-Many relationship)
        // Configure the collection using the backing field
        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey("SaleId")
            .OnDelete(DeleteBehavior.Cascade);

        // Configure to use the backing field _items for the Items property
        builder.Metadata.FindNavigation(nameof(Sale.Items))!
            .SetField("_items");

        // Computed properties - we ignore these as they're calculated
        builder.Ignore(s => s.TotalQuantity);
        builder.Ignore(s => s.Subtotal);
        builder.Ignore(s => s.TotalDiscount);
        builder.Ignore(s => s.TotalAmount);

        // Domain Events - These won't be stored directly in DB
        builder.Ignore(s => s.DomainEvents);
    }
}
