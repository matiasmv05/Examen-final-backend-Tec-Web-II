using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        private readonly IDapperContext _dapper;
        public ProductRepository(AmazonContext context, IDapperContext dapper) : base(context)
        {
            _dapper = dapper;
            //_context = context;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
                return await _entities.Where(p => p.Id == id)
                    .Include(p => p.Seller)
                    .FirstOrDefaultAsync(p => p.Id == id);

        }

        //public async Task<IEnumerable<Product>> GetAllAsync()
        //{
        //    return await _context.Products
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<Product>> GetBySellerAsync(int sellerId)
        {
            return await _entities
                .Where(p => p.SellerId == sellerId )
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _entities
                .Where(p => p.Category == category )
                .ToListAsync();
        }

        //public async Task AddAsync(Product product)
        //{
        //   await _context.Products.AddAsync(product);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task UpdateAsync(Product product)    
        //{
        //    _context.Products.Update(product);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteAsync(Product product)    
        //{
        //    _context.Products.Remove(product);
        //    await _context.SaveChangesAsync();
        //}
    }
}
