using AutoMapper;
using DeveloperStore.Application.Common.DTOs;
using DeveloperStore.Application.Sales.Queries.GetAllSales;
using DeveloperStore.Application.Sales.Queries.GetSaleById;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for mapping between domain entities and DTOs
/// Handles complex mapping logic for Sale aggregate
/// </summary>
public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        // Sale entity to SaleDto (existing command DTOs)
        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer.CustomerId))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Branch.BranchId))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.BranchLocation, opt => opt.MapFrom(src => src.Branch.Location))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.TotalAmount.Currency))
            .ForMember(dest => dest.SaleLevelDiscountAmount, opt => opt.MapFrom(src => src.SaleLevelDiscount.Amount))
            .ForMember(dest => dest.SaleLevelDiscountCurrency, opt => opt.MapFrom(src => src.SaleLevelDiscount.Currency));

        // SaleItem entity to SaleItemDto (existing command DTOs)
        CreateMap<SaleItem, Common.DTOs.SaleItemDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductCategory, opt => opt.MapFrom(src => src.Product.Category))
            .ForMember(dest => dest.ProductUnitPrice, opt => opt.MapFrom(src => src.Product.UnitPrice.Amount))
            .ForMember(dest => dest.ProductUnitPriceCurrency, opt => opt.MapFrom(src => src.Product.UnitPrice.Currency))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.UnitPriceCurrency, opt => opt.MapFrom(src => src.UnitPrice.Currency))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Discount.Amount))
            .ForMember(dest => dest.DiscountCurrency, opt => opt.MapFrom(src => src.Discount.Currency))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.LineTotal.Amount))
            .ForMember(dest => dest.TotalPriceCurrency, opt => opt.MapFrom(src => src.LineTotal.Currency));

        // **NEW QUERY MAPPINGS**

        // Money value object to MoneyDto
        CreateMap<Money, MoneyDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

        // CustomerInfo value object to CustomerDto
        CreateMap<CustomerInfo, CustomerDto>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        // BranchInfo value object to BranchDto
        CreateMap<BranchInfo, BranchDto>()
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

        // ProductInfo value object to ProductDto
        CreateMap<ProductInfo, ProductDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));

        // SaleItem entity to query GetSaleByIdSaleItemDto
        CreateMap<SaleItem, Sales.Queries.GetSaleById.GetSaleByIdSaleItemDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // Sale entity to GetSaleByIdResponse
        CreateMap<Sale, GetSaleByIdResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SaleNumber, opt => opt.MapFrom(src => src.SaleNumber))
            .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(src => src.SaleDate))
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
            .ForMember(dest => dest.Branch, opt => opt.MapFrom(src => src.Branch))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src => src.TotalQuantity))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
            .ForMember(dest => dest.SaleLevelDiscount, opt => opt.MapFrom(src => src.SaleLevelDiscount))
            .ForMember(dest => dest.TotalDiscount, opt => opt.MapFrom(src => src.TotalDiscount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.IsCancelled))
            .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.CancellationReason))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // Sale entity to GetAllSalesResponse
        CreateMap<Sale, GetAllSalesResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SaleNumber, opt => opt.MapFrom(src => src.SaleNumber))
            .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(src => src.SaleDate))
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
            .ForMember(dest => dest.Branch, opt => opt.MapFrom(src => src.Branch))
            .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src => src.TotalQuantity))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
            .ForMember(dest => dest.TotalDiscount, opt => opt.MapFrom(src => src.TotalDiscount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.IsCancelled))
            .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.CancellationReason))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}
