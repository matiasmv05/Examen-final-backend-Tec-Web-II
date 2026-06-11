using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Interface;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<Order> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllDapperAsync(int limit = 10);
    Task<IEnumerable<Order>> GetAllAsync();
    //Task<IEnumerable<Order>> GetAllAsync(int limit = 10);
    Task<Order> GetUserCartAsync(int userId);
    Task<IEnumerable<Order>> GetAllOderUserAsync(int userId);
    Task<IEnumerable<Order>> GetSellerSalesAsync(int sellerId);
    Task<IEnumerable<ReporteMensualVentasResponse>> GetReporteMensualVentas();
    Task<IEnumerable<BoardStatsResponse>> GetBoardStats();
    Task<IEnumerable<TopProductosVendidosResponse>> GetTopProductosVendidos();
    Task<IEnumerable<LowStockProductResponse>> GetLowStockProductResponse();
    Task<IEnumerable<TopUsersBySpendingResponse>> GetTopUsersBySpending();
    //Task AddAsync(Order order);
    //Task UpdateAsync(Order order);
    //Task DeleteAsync(Order order);

}
