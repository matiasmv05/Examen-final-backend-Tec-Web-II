using Amazon.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment> GetByIdAsync(int id);
        Task<Payment> GetByOrderIdAsync(int orderId);
        //Task AddAsync(Payment payment);
        //Task UpdateAsync(Payment payment);    
        //Task DeleteAsync(Payment payment);
    }
}
