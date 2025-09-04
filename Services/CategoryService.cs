using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Cryptography;
using AutoMapper;


namespace E_CommerceSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepo categoryRepo, IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }
        public IEnumerable<Category> GetAllCategories(int pageNumber, int pageSize)
        {
            // Base query
            var query = _categoryRepo.GetAllCategories();
            // Pagination
            var pagedCategories = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return pagedCategories;
        }
        public Category GetCategoryById(int cid)
        {
            var category = _categoryRepo.GetCategoryById(cid);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {cid} not found.");
            return category;
        }
        public void AddCategory(CategoryDTO categoryDTO)
        {
            // check if category already exist 
            var existingCategory = _categoryRepo.GetCategoryByName(categoryDTO.Name);
            if (existingCategory != null)
                throw new InvalidOperationException($"Category with name {categoryDTO.Name} already exists.");
            // Map DTO to entity
            var category = _mapper.Map<Category>(categoryDTO);
            _categoryRepo.AddCategory(category);
        }
        public void UpdateCategory(int cid, CategoryDTO categoryDTO)
        {
            var existingCategory = _categoryRepo.GetCategoryById(cid);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Category with ID {cid} not found.");
            // Map updated fields from DTO to the existing entity
            _mapper.Map(categoryDTO, existingCategory);
            _categoryRepo.UpdateCategory(existingCategory);
        }
        public void DeleteCategory(int cid)
        {
            var existingCategory = _categoryRepo.GetCategoryById(cid);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Category with ID {cid} not found.");
            _categoryRepo.DeleteCategory(existingCategory);
        }
    }
}