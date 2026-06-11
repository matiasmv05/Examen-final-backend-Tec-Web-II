using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.QueryFilters;

namespace Amazon.Core.Interface 
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrderAsync();
        Task<ResponseData> GetAllOrder(OrderQueryFilter filters);
        Task<Order> GetByIdOrderAsync(int id);
        Task<Order> GetUserCartAsync(int userId);
        Task<Order_Item> InsertProductIntoCart(int productId, int orderId, int quantity);
        Task DeleteItemAsync(int userId, int productId);
        Task<Order_Item> GetOrderItemAsync(int orderId, int productId);
        Task InsertAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Order order);
        Task CreatedOrder(Order order);
        Task<Payment> ProcessPaymentAsync(int userId);
        Task<IEnumerable<Order>> GetAllOderUserAsync(int userId);
        Task<IEnumerable<Order>> GetSellerSalesAsync(int sellerId);

        Task<IEnumerable<ReporteMensualVentasResponse>> GetReporteMensualVentas();
        Task<IEnumerable<BoardStatsResponse>> GetBoardStats();
        Task<IEnumerable<TopProductosVendidosResponse>> GetTopProductosVendidos();
        Task<IEnumerable<LowStockProductResponse>> GetLowStockProductResponse();
        Task<IEnumerable<TopUsersBySpendingResponse>> GetTopUsersBySpending();
    }
}