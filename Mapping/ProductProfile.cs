using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.ProductDTO;

namespace E_CommerceSystem.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            //CreateMap<ProductUpdateDto, Product>()
            //    .ForMember(d => d.Price, opt => opt.Ignore())
            //    .ForMember(d => d.Category, opt => opt.Ignore()) 
            //    .ForMember(d => d.Supplier, opt => opt.Ignore())
            //    .ForAllMembers(opt =>
            //        opt.Condition((src, dest, srcMember) => srcMember != null));

            //CreateMap<Product, ProductReadDto>();

            // This map is what your action needs:
            CreateMap<ProductDTO, Product>()
                .ForMember(d => d.PID, o => o.Ignore())   // PID is DB-generated
                .ForMember(d => d.OverallRating, o => o.MapFrom(_ => 0));

            // If you also use create/update DTOs elsewhere, keep these too:
            // CreateMap<ProductCreateDto, Product>().ForMember(...);
            // CreateMap<ProductUpdateDto, Product>();

            // Read model
            CreateMap<Product, ProductReadDto>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
                .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null));

        }
    }
}
