using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface ICategoryRepo
    {
        void AddCategory(Category category);
        void DeleteCategory(Category category);
        IEnumerable<Category> GetAllCategories();
        Category GetCategoryById(int id);
        Category GetCategoryByName(string name);
        void UpdateCategory(Category category);
    }
}