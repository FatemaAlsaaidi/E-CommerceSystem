using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Cryptography;
using AutoMapper;


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
        public void AddSupplier(SupplierDTO supplierDTO)
        {
            // check if supplier already exist 
            var existingSupplier = _supplierRepo.GetSupplierByName(supplierDTO.Name);
            if (existingSupplier != null)
                throw new InvalidOperationException($"Supplier with name {supplierDTO.Name} already exists.");
            // Map DTO to entity
            var supplier = _mapper.Map<Supplier>(supplierDTO);
            _supplierRepo.AddSupplier(supplier);
        }
        public void UpdateSupplier(int sid, SupplierDTO supplierDTO)
        {
            var existingSupplier = _supplierRepo.GetSupplierById(sid);
            if (existingSupplier == null)
                throw new KeyNotFoundException($"Supplier with ID {sid} not found.");
            // Map updated fields from DTO to the existing entity
            _mapper.Map(supplierDTO, existingSupplier);
            _supplierRepo.UpdateSupplier(existingSupplier);
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