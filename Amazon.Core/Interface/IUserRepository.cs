using Amazon.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface IUserRepository : IBaseRepository<User>
    {
        //Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        //Task AddAsync(User user);
        //Task UpdateAsync(User user);    
        //Task DeleteAsync(User user);
        Task UpdateWalletAsync(int userId, decimal amount);
        Task SoftDeleteAsync(int id);
    }
}
