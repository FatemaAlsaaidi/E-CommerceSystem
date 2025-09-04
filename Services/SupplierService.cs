using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Cryptography;
using AutoMapper;

using AutoMapper;
using AutoMapper.QueryableExtensions; // <-- needed for ProjectTo
using Microsoft.EntityFrameworkCore;  // <-- for ToListAsync


namespace E_CommerceSystem.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepo _supplierRepo;
        private readonly IMapper _mapper;
        public SupplierService(ISupplierRepo supplierRepo, IMapper mapper)
        {
            _supplierRepo = supplierRepo;
            _mapper = mapper;
        }
        public IEnumerable<Supplier> GetAllSuppliers(int pageNumber, int pageSize)
        {
            // Base query
            var query = _supplierRepo.GetAllSuppliers();
            // Pagination
            var pagedSuppliers = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return pagedSuppliers;
        }
        public Supplier GetSupplierById(int sid)
        {
            var supplier = _supplierRepo.GetSupplierById(sid);
            if (supplier == null)
                throw new KeyNotFoundException($"Supplier with ID {sid} not found.");
            return supplier;
        }
        public void AddSupplier(SupplierDTO dto)
        {
            var exists = _supplierRepo.GetSupplierByName(dto.Name);
            if (exists == null) throw new InvalidOperationException($"Supplier with name {dto.Name} already exists.");

            var supplier = new Supplier
            {
                Name = dto.Name,
                ContactEmail = dto.ContactInfo   // dto only has ContactInfo
                                                 // Phone stays null unless you add it to the DTO
            };
            _supplierRepo.AddSupplier(supplier);
        }
        public void UpdateSupplier(int sid, SupplierDTO dto)
        {
            var supplier = _supplierRepo.GetSupplierById(sid)
        ?? throw new KeyNotFoundException($"Supplier with ID {sid} not found.");

            supplier.Name = dto.Name;
            supplier.ContactEmail = dto.ContactInfo;
            _supplierRepo.UpdateSupplier(supplier);
        }
        public void DeleteSupplier(int sid)
        {
            var existingSupplier = _supplierRepo.GetSupplierById(sid);
            if (existingSupplier == null)
                throw new KeyNotFoundException($"Supplier with ID {sid} not found.");
            _supplierRepo.DeleteSupplier(existingSupplier);
        }
    }
}