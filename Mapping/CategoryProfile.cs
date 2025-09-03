using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.CategoryDTO;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();
        CreateMap<CategoryReadDto, CategoryReadDto>();
    }
}