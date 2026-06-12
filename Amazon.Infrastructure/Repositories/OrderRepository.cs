using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Amazon.Infrastructure.Data;
using Amazon.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Repositories
{


    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        private readonly IDapperContext _dapper;
        public OrderRepository(AmazonContext context, IDapperContext dapper) : base(context)
        {
            _dapper = dapper;
            //_context = context;
        }

        public async Task<Order> GetByIdAsync(int id)
       {
           return await _entities
                  .Include(o => o.User)
                  .Include(o => o.OrderItems)
                  .ThenInclude(oi => oi.Product)
                  .Include(o => o.Payment)
                  .FirstOrDefaultAsync(o => o.Id == id);
       }

        public async Task<IEnumerable<Order>> GetAllDapperAsync(int limit=10)
        {
            try
            {
                var sql = OrderQueries.OrderQueryMySQL;
                return await _dapper.QueryAsync<Order>(sql, new { Limit = limit });
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                var posts = await _entities
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Payment)
                    .ToListAsync();
                return posts;
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        public async Task<Order> GetUserCartAsync(int userId)
        {
            return await _entities
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Cart");
        }

        public async Task<IEnumerable<Order>> GetAllOderUserAsync(int userId)
        {
            return await _entities
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetSellerSalesAsync(int sellerId)
        {
            // Retorna órdenes que no sean carrito y que contengan al menos un producto del seller
            return await _entities
                .Where(o => o.Status != "Cart" && o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                .Include(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId)) // Solo traer los items del seller
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Include(o => o.User) // Para ver quién compró
                .ToListAsync();
        }

        public async Task<IEnumerable<ReporteMensualVentasResponse>> GetReporteMensualVentas()
        {
            try
            {
                var sql = OrderQueries.GetReporteMensualVentas;

                return await _dapper.QueryAsync<ReporteMensualVentasResponse>(sql);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }
        public async Task<IEnumerable<BoardStatsResponse>> GetBoardStats()
        {
            try
            {
                var sql = OrderQueries.GetBoardStats;

                return await _dapper.QueryAsync<BoardStatsResponse>(sql);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }
        public async Task<IEnumerable<TopProductosVendidosResponse>> GetTopProductosVendidos()
        {
            try
            {
                var sql = OrderQueries.GetTopProductosVendidos;

                return await _dapper.QueryAsync<TopProductosVendidosResponse>(sql);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }
        public async Task<IEnumerable<LowStockProductResponse>> GetLowStockProductResponse()
        {
            try
            {
                var sql = OrderQueries.GetProductsLowStock;

                return await _dapper.QueryAsync<LowStockProductResponse>(sql);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }
        public async Task<IEnumerable<TopUsersBySpendingResponse>> GetTopUsersBySpending()
        {
            try
            {
                var sql = OrderQueries.GetTopUsersBySpending;

                return await _dapper.QueryAsync<TopUsersBySpendingResponse>(sql);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        //public async Task AddAsync(Order order)
        //{
        //    try
        //    {
        //        _context.Orders.Add(order);
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        var detalle = ex.InnerException?.Message ?? ex.GetBaseException().Message;
        //        throw new Exception("Error al crear orden: " + detalle);
        //    }

        //}

        //public async Task UpdateAsync(Order order)
        //{
        //    _context.Orders.Update(order);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteAsync(Order order)
        //{
        //    _context.Orders.Remove(order);
        //    await _context.SaveChangesAsync();
        //}
    }
}
