using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Exceptions;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
using System.Net;

namespace Amazon.Core.Services
{
    /// <summary>
    /// Servicio de aplicación para gestionar la lógica de negocio de órdenes en el sistema Amazon
    /// </summary>
    /// <remarks>
    /// Este servicio orquesta las operaciones relacionadas con órdenes, carritos de compra y pagos.
    /// Utiliza el patrón Unit of Work para gestionar transacciones y consistencia de datos.
    /// </remarks>
    public class OrderService : IOrderService
    {
        //private readonly IOrderRepository _orderRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly IProductRepository _productRepository;
        //private readonly IPaymentRepository _paymentRepository;

        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de órdenes
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para gestionar transacciones y acceso a datos</param>
        public OrderService(
            //IOrderRepository orderRepository,
            //IUserRepository userRepository,
            //IProductRepository productRepository,
            //IPaymentRepository paymentRepository)
             IUnitOfWork unitOfWork)
            {
            //_orderRepository = orderRepository;
            //_userRepository = userRepository;
            //_productRepository = productRepository;
            //_paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }


        /// //////////////////////////////////////////////////////////////////
        /// <summary>
        /// Obtiene todas las órdenes del sistema
        /// </summary>
        /// <returns>Colección de todas las órdenes registradas</returns>
        public async Task<IEnumerable<Order>> GetAllOrderAsync()
        {
            return await _unitOfWork.OrderRepository.GetAllAsync();
        }

        public async Task<ResponseData> GetAllOrder(OrderQueryFilter filters)
        {
            var orders = await _unitOfWork.OrderRepository.GetAllAsync();

            if (filters.UserId > 0) 
            {
                orders = orders.Where(x => x.UserId == filters.UserId);
            }

            if (filters.TotalAmount != null)
            {
                orders = orders.Where(x => x.TotalAmount == filters.TotalAmount.Value);
            }

            var pagedOrders = PagedList<object>.Create(orders, filters.PageNumber, filters.PageSize);
            if (pagedOrders.Any())
            {
                return new ResponseData()
                {
                    Messages = new Message[] { new() { Type = "Information", Description = "Registros de orders recuperados correctamente" } },
                    Pagination = pagedOrders,
                    StatusCode = HttpStatusCode.OK
                };
            }
            else
            {
                return new ResponseData()
                {
                    Messages = new Message[] { new() { Type = "Warning", Description = "No fue posible recuperar la cantidad de registros" } },
                    Pagination = pagedOrders,
                    StatusCode = HttpStatusCode.NotFound
                };
            }
        }

        /// //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Obtiene una orden específica por su identificador único
        /// </summary>
        /// <param name="id">Identificador único de la orden</param>
        /// <returns>Orden correspondiente al ID especificado</returns>
        public async Task<Order> GetByIdOrderAsync(int id)
        {
            return await _unitOfWork.OrderRepository.GetByIdAsync(id);
        }

        /// //////////////////////////////////////////////////////////////////
        /// <summary>
        /// Obtiene el carrito de compras activo de un usuario
        /// </summary>
        /// <param name="userId">Identificador único del usuario</param>
        /// <returns>Carrito de compras en estado "Cart" del usuario</returns>
        public async Task<Order> GetUserCartAsync(int userId)
        {
            return await _unitOfWork.OrderRepository.GetUserCartAsync(userId);
        }
        /// <summary>
        /// Inserta una nueva orden en el sistema
        /// </summary>
        /// <param name="order">Orden a ser insertada</param>
        /// <remarks>
        /// Guarda los cambios en la base de datos después de insertar la orden
        /// </remarks>
        public async Task InsertAsync(Order order)
        {

                await _unitOfWork.OrderRepository.Add(order);
                await _unitOfWork.SaveChangesAsync();


        }

        /// <summary>
        /// Actualiza una orden existente en el sistema
        /// </summary>
        /// <param name="order">Orden con los datos actualizados</param>
        /// <remarks>
        /// Guarda los cambios en la base de datos después de actualizar la orden
        /// </remarks>
        public async Task UpdateAsync(Order order)  
        {
            await _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

        }
        /// <summary>
        /// Elimina una orden del sistema
        /// </summary>
        /// <param name="order">Orden a ser eliminada</param>
        /// <remarks>
        /// Guarda los cambios en la base de datos después de eliminar la orden
        /// </remarks>
        public async Task DeleteAsync(Order order)  
        {
            await _unitOfWork.OrderRepository.Delete(order.Id);
            await _unitOfWork.SaveChangesAsync();

        }
        /// <summary>
        /// Obtiene un producto por su identificador único
        /// </summary>
        /// <param name="id">Identificador único del producto</param>
        /// <returns>Producto correspondiente al ID especificado</returns>
        public async Task<Product> GetByIdProductAsync(int id)
        {
            return await _unitOfWork.ProductRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Crea una nueva orden completando la información necesaria
        /// </summary>
        /// <param name="order">Orden a ser creada</param>
        /// <remarks>
        /// Completa automáticamente los precios unitarios de los items basándose en los productos
        /// y guarda la orden en la base de datos
        /// </remarks>
        public async Task CreatedOrder(Order order)
{
    var user = await _unitOfWork.UserRepository.GetById(order.UserId); // FIX
    order.User = user;
    foreach (var item in order.OrderItems)
    {
        var product = await GetByIdProductAsync(item.ProductId);
        item.UnitPrice = product.Price;
    }
    await _unitOfWork.OrderRepository.Add(order);
    await _unitOfWork.SaveChangesAsync(); // solo una vez
}
        /// <summary>
        /// Inserta un producto en el carrito de compras de un usuario
        /// </summary>
        /// <param name="productId">Identificador del producto a agregar</param>
        /// <param name="orderId">Identificador de la orden (carrito)</param>
        /// <param name="quantity">Cantidad del producto a agregar</param>
        /// <returns>Item de orden creado con el producto agregado</returns>
        public async Task<Order_Item> InsertProductIntoCart(int productId, int orderId, int quantity)
{
    var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
    if (order == null)
        throw new BussinesException("Orden no encontrada", HttpStatusCode.NotFound);

    var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
    if (product == null)
        throw new BussinesException("Producto no encontrado", HttpStatusCode.NotFound);

    // Regla de negocio: un vendedor no puede comprar sus propios productos
    if (product.SellerId == order.UserId)
        throw new BussinesException("No puedes comprar tus propios productos", HttpStatusCode.BadRequest);

    // Verificar si ya existe ese producto en esta orden
    var existingItem = await _unitOfWork.OrderItemRepository
        .GetOrderItemAsync(orderId, productId);

    if (existingItem != null)
    {
        // Ya existe: solo sumar cantidad
        existingItem.Quantity += quantity;
        await _unitOfWork.OrderItemRepository.Update(existingItem);
        await _unitOfWork.SaveChangesAsync();
        return existingItem;
    }

    // No existe: crear nuevo item
    var newItem = new Order_Item
    {
        ProductId = productId,
        Quantity = quantity,
        UnitPrice = product.Price,
        OrderId = order.Id
    };

    await _unitOfWork.OrderItemRepository.AddProductIntoCart(newItem);
    await _unitOfWork.SaveChangesAsync();
    return newItem;
}

        /// <summary>
        /// Elimina un producto del carrito de compras de un usuario
        /// </summary>
        /// <param name="userId">Identificador del usuario</param>
        /// <param name="productId">Identificador del producto a eliminar</param>
        public async Task DeleteItemAsync(int userId, int productId)
        {

            var Carrito = await GetUserCartAsync(userId);
            var item = await _unitOfWork.OrderItemRepository.GetOrderItemAsync(Carrito.Id, productId);
            await _unitOfWork.OrderItemRepository.Delete(item.Id);
            await _unitOfWork.SaveChangesAsync();

        }

        /// <summary>
        /// Obtiene un item específico de una orden
        /// </summary>
        /// <param name="orderId">Identificador de la orden</param>
        /// <param name="productId">Identificador del producto</param>
        /// <returns>Item de orden correspondiente a los identificadores especificados</returns>
        public async Task<Order_Item> GetOrderItemAsync(int orderId, int productId)
        {
            return await _unitOfWork.OrderItemRepository.GetOrderItemAsync(orderId, productId);
        }

        /// <summary>
        /// Procesa el pago del carrito de compras de un usuario
        /// </summary>
        /// <param name="userId">Identificador del usuario que realiza el pago</param>
        /// <returns>Pago procesado con la información de la transacción</returns>
        /// <remarks>
        /// Este método realiza las siguientes operaciones en una transacción:
        /// 1. Valida el stock de los productos
        /// 2. Transfiere fondos del usuario a los vendedores
        /// 3. Actualiza el inventario de productos
        /// 4. Cambia el estado de la orden a "Paid"
        /// 5. Crea el registro de pago
        /// 
        /// En caso de error, se realiza rollback de toda la transacción
        /// </remarks>
        /// <exception cref="Exception">Se lanza cuando hay stock insuficiente o error en el procesamiento</exception>
        public async Task<Payment> ProcessPaymentAsync(int userId)
{
    await _unitOfWork.BeginTransaccionAsync(); // FALTA ESTO
    try
    {
        var cart = await GetUserCartAsync(userId);
        var user = await _unitOfWork.UserRepository.GetById(userId);
        var totalAmount = cart.OrderItems.Sum(item => item.Subtotal);

        // Validar saldo ANTES de operar
        if (user.Billetera < totalAmount)
            throw new BussinesException(
            "Saldo insuficiente en billetera",
             HttpStatusCode.UnprocessableEntity); 

        // Validar stock de todos los items ANTES de modificar nada
        foreach (var item in cart.OrderItems)
        {
            var product = await GetByIdProductAsync(item.ProductId);
            if (product.Stock < item.Quantity)
                throw new BussinesException(
                    $"Stock insuficiente para {product.Name}. " +
                    $"Disponible: {product.Stock}, Solicitado: {item.Quantity}",
                    HttpStatusCode.UnprocessableEntity);
        }

        // Ahora sí modificar
        user.Billetera -= totalAmount;
        await _unitOfWork.UserRepository.Update(user);

        foreach (var item in cart.OrderItems)
        {
            var product = await GetByIdProductAsync(item.ProductId);
            product.Stock -= item.Quantity;
            var seller = await _unitOfWork.UserRepository.GetById(product.SellerId);
            seller.Billetera += item.Subtotal;
            await _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.UserRepository.Update(seller);
        }

        var payment = new Payment
        {
            OrderId = cart.Id,
            Status = "Completed",
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.PaymentRepository.Add(payment);

        cart.Status = "Paid";
        cart.TotalAmount = totalAmount;
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.OrderRepository.Update(cart);

        await _unitOfWork.CommitAsync(); // único SaveChanges + commit
        return payment;
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}

        /// <summary>
        /// Obtiene todas las órdenes de un usuario específico
        /// </summary>
        /// <param name="userId">Identificador único del usuario</param>
        /// <returns>Colección de órdenes pertenecientes al usuario</returns>
        public Task<IEnumerable<Order>> GetAllOderUserAsync(int userId)
        {
            return _unitOfWork.OrderRepository.GetAllOderUserAsync(userId);
        }

        /// <summary>
        /// Obtiene las ventas de un vendedor (órdenes que contienen sus productos)
        /// </summary>
        /// <param name="sellerId">Identificador único del vendedor</param>
        /// <returns>Colección de órdenes que tienen productos del vendedor</returns>
        public async Task<IEnumerable<Order>> GetSellerSalesAsync(int sellerId)
        {
            return await _unitOfWork.OrderRepository.GetSellerSalesAsync(sellerId);
        }

        /// <summary>
        /// Obtiene el reporte mensual de ventas del sistema
        /// </summary>
        /// <returns>Reporte con estadísticas de ventas agrupadas por mes</returns>
        public async Task<IEnumerable<ReporteMensualVentasResponse>> GetReporteMensualVentas()
        {
            return await _unitOfWork.OrderRepository.GetReporteMensualVentas();
        }

        /// <summary>
        /// Obtiene estadísticas del dashboard administrativo
        /// </summary>
        /// <returns>Métricas generales del sistema para el panel de control</returns>
        public async Task<IEnumerable<BoardStatsResponse>> GetBoardStats()
        {
            return await _unitOfWork.OrderRepository.GetBoardStats();
        }

        /// <summary>
        /// Obtiene los productos más vendidos en el sistema
        /// </summary>
        /// <returns>Lista de productos ordenados por cantidad de ventas</returns>

        public async Task<IEnumerable<TopProductosVendidosResponse>> GetTopProductosVendidos()
        {
            return await _unitOfWork.OrderRepository.GetTopProductosVendidos();
        }

        /// <summary>
        /// Obtiene productos con stock bajo que requieren atención
        /// </summary>
        /// <returns>Lista de productos con stock por debajo del nivel mínimo</returns>
        public async Task<IEnumerable<LowStockProductResponse>> GetLowStockProductResponse()
        {
            return await _unitOfWork.OrderRepository.GetLowStockProductResponse();

        }

        /// <summary>
        /// Obtiene los usuarios que más han gastado en el sistema
        /// </summary>
        /// <returns>Lista de usuarios ordenados por monto total gastado</returns>
        public async Task<IEnumerable<TopUsersBySpendingResponse>> GetTopUsersBySpending()
        {
            return await _unitOfWork.OrderRepository.GetTopUsersBySpending();
        }
    }
}
