using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class CategoryRepo : ICategoryRepo
    {
        // database context injection 
        public ApplicationDbContext _context;

        public CategoryRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // get all categories
        public IEnumerable<Category> GetAllCategories()
        {
            try
            {
                return _context.Categories.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // get category by id
        public Category GetCategoryById(int id)
        {
            try
            {
                return _context.Categories.FirstOrDefault(c => c.CategoryId == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // Get Category By Name
        public Category GetCategoryByName(string name)
        {
            try
            {
                return _context.Categories.FirstOrDefault(c => c.Name == name);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }

        }

        // Add Category

        public void AddCategory(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // Delete Category
        public void DeleteCategory(Category category)
        {
            try
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // Update category
        public void UpdateCategory(Category category)
        {
            try
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
    }

}
