using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface ISupplierRepo
    {
        void AddSupplier(Supplier supplier);
        void DeleteSupplier(Supplier supplier);
        IEnumerable<Supplier> GetAllSuppliers();
        Supplier GetSupplierById(int id);
        Supplier GetSupplierByName(string name);
        void UpdateSupplier(Supplier supplier);
    }
}