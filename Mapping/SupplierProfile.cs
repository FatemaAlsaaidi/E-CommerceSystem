using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.SupplierDTO;

namespace E_CommerceSystem.Mapping
{


    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<SupplierCreateDto, Supplier>();
            CreateMap<SupplierUpdateDto, Supplier>();
            CreateMap<Supplier, SupplierReadDto>();
        }
    }
}
