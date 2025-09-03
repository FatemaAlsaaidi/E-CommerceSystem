using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface ICategoryService
    {
        void AddCategory(CategoryDTO categoryDTO);
        void DeleteCategory(int cid);
        IEnumerable<Category> GetAllCategories(int pageNumber, int pageSize);
        Category GetCategoryById(int cid);
        void UpdateCategory(int cid, CategoryDTO categoryDTO);
    }
}