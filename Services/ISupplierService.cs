using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface ISupplierService
    {
        void AddSupplier(SupplierDTO supplierDTO);
        void DeleteSupplier(int sid);
        IEnumerable<Supplier> GetAllSuppliers(int pageNumber, int pageSize);
        Supplier GetSupplierById(int sid);
        void UpdateSupplier(int sid, SupplierDTO supplierDTO);
    }
}