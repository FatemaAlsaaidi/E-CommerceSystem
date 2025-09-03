using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.ProductDTO;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductReadDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.SupplierName, opt => opt.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null));

        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();
    }
}
