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
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly IDapperContext _dapper;
        public UserRepository(AmazonContext context, IDapperContext dapper) : base(context)
        {
            _dapper = dapper;
            //_context = context;
        }

        //public async Task<User> GetByIdAsync(int id)
        //{
        //    return await _context.Users.FindAsync(id);    
        //}

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _entities.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _entities.ToListAsync();
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        //public async Task AddAsync(User user)
        //{
        //    await _context.Users.AddAsync(user);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task UpdateAsync(User user)    
        //{
        //    _context.Users.Update(user);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteAsync(User user)
        //{
        //    _context.Users.Remove(user);
        //    await _context.SaveChangesAsync();

        //}

        public async Task UpdateWalletAsync(int userId, decimal amount)
        {
            var user = await _entities.FindAsync(userId);
            if (user != null)
            {
                user.Billetera += amount;
                _entities.Update(user);
            }
        }

        public async Task SoftDeleteAsync(int id)
{
          var user = await _entities.FindAsync(id);
          if (user != null)
          {
             user.IsActive = false;
             _entities.Update(user);
          }

}
       
    }
}
