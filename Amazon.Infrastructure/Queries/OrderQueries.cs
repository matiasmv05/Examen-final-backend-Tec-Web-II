using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Queries
{
    public static class OrderQueries
    {
        public static string OrderQuerySqlServer = @"
            SELECT o.Id, o.UserId, o.TotalAmount, o.CreatedAt,
                   oi.Id AS OrderItemId, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                   p.Id AS ProductId, p.Name, p.Description, p.Price, p.Stock, p.Category, p.SellerId, p.ImageUrl, p.CreatedAt,
                   u.Id AS UserId, u.Name AS UserName, u.Email, u.Billetera, u.CreatedAt AS UserCreatedAt
            FROM [Order] o
            INNER JOIN Order_Item oi ON o.Id = oi.OrderId
            INNER JOIN Product p ON oi.ProductId = p.Id
            INNER JOIN [User] u ON o.UserId = u.Id
            WHERE o.Id = @OrderId
            OFFSET 0 ROWS FETCH NEXT @Limit ROWS ONLY;";

        public static string GetBoardStats = @"
         SELECT 
        -- Estadísticas de usuarios
        (SELECT COUNT(*) FROM Users) as TotalUsuarios,
        (SELECT COUNT(*) FROM Users WHERE Billetera > 100) as UsuariosConAltoSaldo,
        
        -- Estadísticas de productos
        (SELECT COUNT(*) FROM Products) as TotalProductos,
        (SELECT COUNT(*) FROM Products WHERE Stock = 0) as ProductosSinStock,
        
        -- Estadísticas de ordenes
        (SELECT COUNT(*) FROM Orders) as TotalOrdenes,
        (SELECT COUNT(*) FROM Orders WHERE Status = 'Paid') as OrdenesPagadas,
        (SELECT COUNT(*) FROM Orders WHERE Status = 'Cart') as CarritosActivos,
        
        -- Estadísticas financieras
        (SELECT SUM(TotalAmount) FROM Orders WHERE Status = 'Paid') as IngresosTotales,
        (SELECT AVG(TotalAmount) FROM Orders WHERE Status = 'Paid') as TicketPromedio,
        (SELECT SUM(Billetera) FROM Users) as SaldoTotalUsuarios;";

        public static string GetTopProductosVendidos = @"
        SELECT 
        p.Id,
        p.Name,
        p.Category,
        SUM(oi.Quantity) as TotalVendido,
        SUM(oi.Quantity * oi.UnitPrice) as IngresosTotales
        FROM Products p
        JOIN Order_Items oi ON p.Id = oi.ProductId
        JOIN Orders o ON oi.OrderId = o.Id
        WHERE o.Status = 'Paid'
        GROUP BY p.Id, p.Name, p.Category
        ORDER BY TotalVendido DESC;";

        public static string GetReporteMensualVentas = @"
        SELECT 
        YEAR(o.UpdatedAt) as Año,
        MONTH(o.UpdatedAt) as Mes,
        DATENAME(MONTH, o.UpdatedAt) as NombreMes,
        COUNT(DISTINCT o.Id) as OrdenesCompletadas,
        COUNT(oi.Id) as ProductosVendidos,
        SUM(o.TotalAmount) as IngresosTotales,
        AVG(o.TotalAmount) as TicketPromedio
        FROM Orders o
        JOIN Order_Items oi ON o.Id = oi.OrderId
        WHERE o.Status = 'Paid'
        GROUP BY YEAR(o.UpdatedAt), MONTH(o.UpdatedAt), DATENAME(MONTH, o.UpdatedAt)
        ORDER BY Año DESC, Mes DESC;";

        public static string GetTopUsersBySpending = @"
        SELECT top 1
        u.Id,
        u.Name,
        u.Email,
        SUM(o.TotalAmount) as TotalGastado,
        COUNT(o.Id) as TotalOrdenes
        FROM Users u
        JOIN Orders o ON u.Id = o.UserId
        WHERE o.Status = 'Paid'
        GROUP BY u.Id, u.Name, u.Email
        ORDER BY TotalGastado DESC;";

        public static string GetProductsLowStock = @"
        SELECT 
        Id,
        Name,
        Category,
        Price,
        Stock
        FROM Products
        WHERE Stock <= 10
        ORDER BY Stock ASC;";
    }
}
