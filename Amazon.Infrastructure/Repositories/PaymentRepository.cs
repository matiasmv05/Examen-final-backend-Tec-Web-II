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
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        private readonly IDapperContext _dapper;
        public PaymentRepository(AmazonContext context, IDapperContext dapper) : base(context)
        {
            _dapper = dapper;
            //_context = context;
        }
        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            try
            {
                return await _entities
                    .Include(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(x => x.Order.User)
                    .ToListAsync();
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }
        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _entities.Where(p => p.Id == id)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> GetByOrderIdAsync(int orderId)
        {
            return await _entities.Where(p => p.OrderId == orderId)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

       

        //public async Task AddAsync(Payment payment)
        //{
        //    await _context.Payments.AddAsync(payment);
        //    await _context.SaveChangesAsync();
        //
        //}

        //public async Task UpdateAsync(Payment payment)    
        //{
        //    _context.Payments.Update(payment);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteAsync(Payment payment)   
        //{
        //    _context.Payments.Remove(payment);
        //    await _context.SaveChangesAsync();
        //}
    }
}
