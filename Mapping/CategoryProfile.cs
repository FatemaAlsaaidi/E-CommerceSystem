using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.CategoryDTO;

namespace E_CommerceSystem.Mapping
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();
            CreateMap<CategoryReadDto, CategoryReadDto>();

            // DTO (class) -> Entity
            CreateMap<CategoryDTO, Category>()
                .ForMember(d => d.CategoryId, opt => opt.Ignore()); // ignore on create/update

            // Entity -> Read DTO (record)
            CreateMap<Category, CategoryReadDto>();
        }
    }
}