using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class SupplierRepo : ISupplierRepo
    {
        // database context injection 
        public ApplicationDbContext _context;

        public SupplierRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all suppliers
        public IEnumerable<Supplier> GetAllSuppliers()
        {
            try
            {
                return _context.Suppliers.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // get supplier by id 
        public Supplier GetSupplierById(int id)
        {
            try
            {
                return _context.Suppliers.FirstOrDefault(s => s.SupplierId == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // get supplier by name
        public Supplier GetSupplierByName(string name)
        {
            try
            {
                return _context.Suppliers.FirstOrDefault(s => s.Name == name);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // add supplier
        public void AddSupplier(Supplier supplier)
        {
            try
            {
                _context.Suppliers.Add(supplier);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // Update supplier
        public void UpdateSupplier(Supplier supplier)
        {
            try
            {
                _context.Suppliers.Update(supplier);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        // Delete supplier
        public void DeleteSupplier(Supplier supplier)
        {
            try
            {
                _context.Suppliers.Remove(supplier);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }

        }
    }
}