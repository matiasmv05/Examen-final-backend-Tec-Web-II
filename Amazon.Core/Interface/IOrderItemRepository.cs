using Amazon.Core.Entities;
using Amazon.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IOrderItemRepository : IBaseRepository<Order_Item>
{
        Task<Order_Item> GetByIdAsync(int id);
        Task<Order_Item> GetOrderItemAsync(int orderId, int productId);
        Task AddProductIntoCart(Order_Item orderItem);
        Task DeleteItemAsync(Order_Item order);

    //Task<IEnumerable<Order_Item>> GetAllAsync();
    //Task AddAsync(Order_Item order);
    //Task UpdateAsync(Order_Item order);
    //Task DeleteAsync(Order_Item order);
}

