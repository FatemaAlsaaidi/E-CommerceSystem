using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.SupplierDTO;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        CreateMap<SupplierCreateDto, Supplier>();
        CreateMap<SupplierUpdateDto, Supplier>();
        CreateMap<Supplier, SupplierReadDto>();
    }
}
