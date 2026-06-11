using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository OrderRepository { get; }
        IOrderItemRepository OrderItemRepository { get; }
        IUserRepository UserRepository { get; }
        IProductRepository ProductRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        ISecurityRepository SecurityRepository { get; }

        void SaveChanges();
        Task SaveChangesAsync();

        Task BeginTransaccionAsync();
        Task CommitAsync();
        Task RollbackAsync();

        //Nuevos miembros de Dapper
        IDbConnection? GetDbConnection();
        IDbTransaction? GetDbTransaction();

    }
}
