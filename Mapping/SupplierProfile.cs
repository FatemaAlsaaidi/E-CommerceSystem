using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.CategoryDTO;
using static E_CommerceSystem.Models.SupplierDTO;

namespace E_CommerceSystem.Mapping
{


    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<SupplierCreateDto, Supplier>();
            CreateMap<SupplierUpdateDto, Supplier>();

            CreateMap<SupplierDTO, Supplier>()
               .ForMember(d => d.SupplierId, opt => opt.Ignore())              // ignore on create
               .ForMember(d => d.ContactEmail, opt => opt.MapFrom(s => s.ContactInfo));

            // Entity -> Read DTO (record)
            CreateMap<Supplier, SupplierReadDto>();

        }
    }
}
