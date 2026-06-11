using Amazon.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product> GetByIdAsync(int id);
        //Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetBySellerAsync(int sellerId);
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        //Task AddAsync(Product product);
        //Task UpdateAsync(Product product);    
        //Task DeleteAsync(Product product);
    }
}
