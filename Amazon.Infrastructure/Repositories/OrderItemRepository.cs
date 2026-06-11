using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Amazon.Infrastructure.Repositories
{
    public class OrderItemRepository : BaseRepository<Order_Item>, IOrderItemRepository
    {
        private readonly IDapperContext _dapper;
        public OrderItemRepository(AmazonContext context, IDapperContext dapper) : base(context)
        {
            _dapper = dapper;
            //_context = context;
        }

        public async Task<Order_Item> GetByIdAsync(int id)
        {
            return await _entities.Where(p => p.Id == id)
                .Include(o => o.order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

        }

        public async Task<Order_Item> GetOrderItemAsync(int orderId, int productId)
        {
                 return await _entities
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductId == productId);
        }

        public async Task AddProductIntoCart(Order_Item orderItem)
        {
            _entities.Add(orderItem);
        }

        public async Task DeleteItemAsync(Order_Item order)
        {
            _entities.Remove(order);
        }

        //public async Task<IEnumerable<Order_Item>> GetAllAsync()
        //{
        //    return await _context.Order_Items.ToListAsync();
        //}

        //public async Task AddAsync(Order_Item orderItem)
        //{
        //    await _context.Order_Items.AddAsync(orderItem);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task UpdateAsync(Order_Item orderItem)
        //{
        //    _context.Order_Items.Update(orderItem);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteAsync(Order_Item orderItem)
        //{
        //    _context.Order_Items.Remove(orderItem);
        //    await _context.SaveChangesAsync();
        //}
    }
}
