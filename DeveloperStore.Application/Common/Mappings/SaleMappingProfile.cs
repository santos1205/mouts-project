using AutoMapper;
using DeveloperStore.Application.Common.DTOs;
using DeveloperStore.Domain.Entities;

namespace DeveloperStore.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for mapping between domain entities and DTOs
/// Handles complex mapping logic for Sale aggregate
/// </summary>
public class SaleMappingProfile : Profile
{
  public SaleMappingProfile()
  {
    // Sale entity to SaleDto
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

    // SaleItem entity to SaleItemDto
    CreateMap<SaleItem, SaleItemDto>()
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
  }
}
