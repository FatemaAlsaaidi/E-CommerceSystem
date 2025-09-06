using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

using AutoMapper;
using AutoMapper.QueryableExtensions; // <-- needed for ProjectTo
using Microsoft.EntityFrameworkCore;  // <-- for ToListAsync

namespace E_CommerceSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepo _productRepo;
        private readonly IMapper _mapper;


        public ProductService(IProductRepo productRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
        }


        //    public IEnumerable<Product> GetAllProducts(int pageNumber, int pageSize, string? name = null, decimal? minPrice = null, decimal? maxPrice = null)
        //    {
        //        // Base query
        //        var query = _productRepo.GetAllProducts();

        //        // Apply filters
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            query = query.Where(p => p.ProductName.Contains(name, StringComparison.OrdinalIgnoreCase));
        //        }

        //        if (minPrice.HasValue)
        //        {
        //            query = query.Where(p => p.Price >= minPrice.Value);
        //        }

        //        if (maxPrice.HasValue)
        //        {
        //            query = query.Where(p => p.Price <= maxPrice.Value);
        //        }

        //        // Pagination
        //        var pagedProducts = query
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToList();

        //        return pagedProducts;

        //}

        public IEnumerable<Product> GetAllProducts(int pageNumber, int pageSize, string? name = null, decimal? minPrice = null, decimal? maxPrice = null)
        { 
            return _productRepo.Search(name, minPrice, maxPrice, pageNumber, pageSize); 
        }
        public Product GetProductById(int pid)
        {
            
                var product = _productRepo.GetProductById(pid);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {pid} not found.");
            return product;
            
        }

        public void AddProduct(Product product)
        {
            _productRepo.AddProduct(product);
        }

        public void UpdateProduct(Product product)
        {
            var existing = _productRepo.GetProductById(product.PID)
                          ?? throw new KeyNotFoundException($"Product with ID {product.PID} not found.");

            // Copy allowed fields + **RowVersion** from incoming object to tracked instance:
            existing.ProductName = product.ProductName;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.CategoryId = product.CategoryId;
            existing.SupplierId = product.SupplierId;
            existing.ImageUrl = product.ImageUrl;

            // Apply the incoming RowVersion so EF can compare
            existing.RowVersion = product.RowVersion;

            try
            {
                _productRepo.UpdateProduct(existing);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("The product was modified by someone else. Please reload and try again.");
            }
        }


        public Product GetProductByName(string productName)
        {
            var product = _productRepo.GetProductByName(productName);
            if (product == null)
                throw new KeyNotFoundException($"Product with Nmae {productName} not found.");
            return product;
        }

        

    }
}
