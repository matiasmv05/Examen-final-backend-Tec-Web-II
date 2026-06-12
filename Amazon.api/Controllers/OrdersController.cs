using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
using Amazon.infrastructure.DTOs;
using Amazon.Infrastructure.DTOs;
using Amazon.Infrastructure.Repositories;
using Amazon.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace Amazon.Api.Controllers
{
    /// <summary>
    /// Controlador para gestionar todas las operaciones relacionadas con órdenes y carritos de compra
    /// </summary>
    /// <remarks>
    /// Este controlador permite:
    /// - Gestionar órdenes completas
    /// - Manejar carritos de compra
    /// - Procesar pagos
    /// - Generar reportes de ventas
    /// </remarks>

    [Authorize] // Autenticación base para todos
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public OrdersController(
            IOrderService orderService,
            IMapper mapper,
            IValidationService validationService
            )
        {
            _orderService = orderService;
            _mapper = mapper;
            _validationService = validationService;
        }

        // Helper reutilizable para extraer el UserId y Rol del token
        private (bool isValid, int tokenUserId, string? userRole) GetTokenClaims()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var tokenUserId))
                return (false, 0, null);

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return (true, tokenUserId, userRole);
        }


        /// <summary>
        /// Recupera todas las órdenes registradas en el sistema
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders
        /// 
        /// Este endpoint devuelve una lista completa de todas las órdenes existentes en el sistema,
        /// incluyendo órdenes completadas, pendientes y carritos activos.
        /// Solo accesible para Administrator.
        /// </remarks>
        /// <returns>Lista paginada de órdenes con sus respectivos datos mapeados al DTO</returns>
        /// <response code="200">Retorna todas las órdenes exitosamente</response>
        /// <response code="403">No autorizado. Se requiere rol Administrator</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<OrderResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery]OrderQueryFilter orderQueryFilter)
        {
            try
            {
                var orders = await _orderService.GetAllOrder(orderQueryFilter);

                var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders.Pagination);

                var pagination = new Pagination
                {
                    TotalCount = orders.Pagination.TotalCount,
                    PageSize = orders.Pagination.PageSize,
                    CurrentPage = orders.Pagination.CurrentPage,
                    TotalPages = orders.Pagination.TotalPages,
                    HasNextPage = orders.Pagination.HasNextPage,
                    HasPreviousPage = orders.Pagination.HasPreviousPage
                };
                var response = new ApiResponse<IEnumerable<OrderResponseDto>>(ordersDto)
                {
                    Pagination = pagination,
                    Messages = orders.Messages
                };

                return StatusCode((int)orders.StatusCode, response);
            }
            catch (Exception err)
            {
                var responsePost = new ResponseData()
                {
                    Messages = new Message[] { new() { Type = "Error", Description = err.Message } },
                };
                return StatusCode(500, responsePost);
            }
        }


        /// <summary>
        /// Obtiene una orden específica por su identificador único
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/5
        /// 
        /// Este endpoint recupera la información completa de una orden específica.
        /// El Administrator puede ver cualquier orden.
        /// Customer/Seller solo pueden ver sus propias órdenes.
        /// </remarks>
        /// <param name="id">Identificador único de la orden (mayor a 0)</param>
        /// <returns>Información detallada de la orden encontrada</returns>
        /// <response code="200">Orden encontrada exitosamente</response>
        /// <response code="400">Error en la validación del ID de la orden</response>
        /// <response code="403">No autorizado para ver esta orden</response>
        /// <response code="404">No se encontró la orden con el ID especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderId(int id)
        {
            try
            {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var orders = await _orderService.GetByIdOrderAsync(id);
                if (orders == null)
                    return NotFound(new ApiResponse<string>("Orden no encontrada"));

                // Verificar propiedad si no es admin
                if (userRole != nameof(RoleType.Administrator) && orders.UserId != tokenUserId)
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new ApiResponse<string>("No tienes permiso para ver esta orden"));

                var orderDto = _mapper.Map<OrderResponseDto>(orders);
                var response = new ApiResponse<OrderResponseDto>(orderDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }


        /// <summary>
        /// Crea una nueva orden simple en el sistema
        /// </summary>
        /// <remarks>
        /// Solo Customer y Seller pueden crear órdenes.
        /// 
        /// Ejemplo de solicitud:
        /// POST /api/Orders
        /// 
        /// Ejemplo de body:
        /// {
        ///   "userId": 1,
        ///   "items": [
        ///     {
        ///       "productId": 5,
        ///       "quantity": 2,
        ///       "unitPrice": 29.99
        ///     }
        ///   ]
        /// }
        /// </remarks>
        /// <param name="crearOrden">Objeto con la información completa de la orden a crear</param>
        /// <returns>Orden creada con su identificador asignado</returns>
        /// <response code="200">Orden creada exitosamente</response>
        /// <response code="400">Error de validación en los datos de entrada</response>
        /// <response code="403">No autorizado. Administrator no puede crear órdenes.</response>
        /// <response code="500">Error interno del servidor al crear la orden</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = $"{nameof(RoleType.Customer)},{nameof(RoleType.Seller)}")]
        [HttpPost]
        public async Task<IActionResult> CreateSimpleOrder([FromBody] CreateOrderRequest CrearOrden)
        {
            try {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                // Forzar que la orden se cree para el usuario del token
                var ordenParaCrear = new Amazon.Core.CustomEntities.CrearOrdenRequest
                {
                    UserId = tokenUserId,
                    OrderItems = CrearOrden.OrderItems
                };

                var validationResult = await _validationService.ValidateAsync(ordenParaCrear);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }
                var order = _mapper.Map<Order>(ordenParaCrear);
                await _orderService.CreatedOrder(order);

                var Orden = await _orderService.GetByIdOrderAsync(order.Id);
                var OrdenRequest = _mapper.Map<OrderResponseDto>(Orden);
                var response = new ApiResponse<OrderResponseDto>(OrdenRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error : {ex.Message}"));
            }
        }


        /// <summary>
        /// Elimina una orden del sistema por su identificador
        /// </summary>
        /// <remarks>
        /// El Administrator puede eliminar cualquier orden.
        /// Customer/Seller solo pueden eliminar sus propias órdenes, y solo si están en estado "Cart".
        /// </remarks>
        /// <param name="id">Identificador único de la orden a eliminar</param>
        /// <response code="204">Orden eliminada correctamente</response>
        /// <response code="403">No autorizado para eliminar esta orden, o no se puede eliminar porque ya está pagada</response>
        /// <response code="404">No se encontró la orden a eliminar</response>
        /// <response code="500">Error interno del servidor al eliminar la orden</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDtoMapper(int id)
        {
            try
            {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var order = await _orderService.GetByIdOrderAsync(id);
                if (order == null)
                    return NotFound(new ApiResponse<string>("Orden no encontrada"));

                // Validar propiedad
                if (userRole != nameof(RoleType.Administrator) && order.UserId != tokenUserId)
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new ApiResponse<string>("No tienes permiso para eliminar esta orden"));

                // Customer/Seller solo pueden eliminar "Cart"
                if (userRole != nameof(RoleType.Administrator) && order.Status != "Cart")
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new ApiResponse<string>("No puedes eliminar una orden que ya fue procesada/pagada"));

                await _orderService.DeleteAsync(order);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error : {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el carrito de compras activo de un usuario específico
        /// </summary>
        /// <remarks>
        /// Solo para uso del Administrator. Para usuarios, usar /api/Orders/my-cart.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("user/{userId}/cart")]
        public async Task<IActionResult> GetUserCartDtoMapper(int userId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = userId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }
                #endregion

                var cart = await _orderService.GetUserCartAsync(userId);
                if(cart == null)
                    return NotFound(new ApiResponse<string>("El usuario no tiene un carrito activo"));

                var cartDto = _mapper.Map<OrderResponseDto>(cart);
                var response = new ApiResponse<OrderResponseDto>(cartDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }


        /// <summary>
        /// Obtiene el carrito de compras activo del usuario autenticado
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        // OrdersController.cs
[Authorize(Roles = $"{nameof(RoleType.Customer)},{nameof(RoleType.Seller)}")]
[HttpPost("my-cart")]
public async Task<IActionResult> CreateMyCart()
{
    try
    {
        var (isValid, tokenUserId, _) = GetTokenClaims();
        if (!isValid)
            return Unauthorized(new ApiResponse<string>("Token inválido"));

        // Verificar si ya tiene carrito activo
        var existing = await _orderService.GetUserCartAsync(tokenUserId);
        if (existing != null)
            return Ok(new ApiResponse<OrderResponseDto>(_mapper.Map<OrderResponseDto>(existing)));

        // Crear carrito vacío
        var order = new Order
        {
            UserId = tokenUserId,
            Status = "Cart",
            UpdatedAt = DateTime.UtcNow,
            TotalAmount = 0,
            OrderItems = new List<Order_Item>()
        };

        await _orderService.InsertAsync(order);

        var created = await _orderService.GetByIdOrderAsync(order.Id);
        return Ok(new ApiResponse<OrderResponseDto>(_mapper.Map<OrderResponseDto>(created)));
    }
    catch (Exception ex)
         {
        return StatusCode(500, new ApiResponse<string>($"Error: {ex.Message}"));
        }
     }

        /// <summary>
        /// Agrega un producto al carrito de compras del usuario autenticado
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderItemDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = $"{nameof(RoleType.Customer)},{nameof(RoleType.Seller)}")]
        [HttpPost("my-cart/items")]
        public async Task<IActionResult> AddItemToMyCart([FromBody] OrderItemRequest request)
        {
            try
            {
                var (isValid, tokenUserId, _) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var cart = await _orderService.GetUserCartAsync(tokenUserId);
                if (cart == null)
                    return BadRequest(new ApiResponse<string>("No tienes un carrito activo, debes crear uno primero"));

                var newItem = await _orderService.InsertProductIntoCart(request.ProductId, cart.Id, request.Quantity);
                var newItemDto = _mapper.Map<OrderItemDto>(newItem);
                var response = new ApiResponse<OrderItemDto>(newItemDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>(ex.Message));
            }
        }


        /// <summary>
        /// Agrega un producto al carrito de compras del usuario
        /// </summary>
        /// <remarks>
        /// Uso para el Administrator o legacy. Para usuarios, usar /api/Orders/my-cart/items.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderItemDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpPost("Product/{productId}/Order/{orderId}/quantity/{quantity}/cart")]
        public async Task<IActionResult> IntroduceItemCart(int productId, int orderId, int quantity)
        {
            try
            {
                var newItem=await _orderService.InsertProductIntoCart(productId,orderId,quantity);
                var newItemDto = _mapper.Map<OrderItemDto>(newItem);
                var response = new ApiResponse<OrderItemDto>(newItemDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        /// <summary>
        /// Elimina un producto específico del carrito de compras del usuario autenticado
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = $"{nameof(RoleType.Customer)},{nameof(RoleType.Seller)}")]
        [HttpDelete("my-cart/items/{productId}")]
        public async Task<IActionResult> RemoveItemFromMyCart(int productId)
        {
            try
            {
                var (isValid, tokenUserId, _) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                await _orderService.DeleteItemAsync(tokenUserId, productId);
                return NoContent();
            }
            catch (Exception ex)
            {
                 return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>(ex.Message));
            }
        }

        /// <summary>
        /// Elimina un producto específico del carrito de compras del usuario
        /// </summary>
        /// <remarks>
        /// Solo uso del Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpDelete("user/{userId}/products/{productId}")]
        public async Task<IActionResult> EliminarItemCarrito(int userId, int productId)
        {
            await _orderService.DeleteItemAsync( userId, productId);
            return NoContent();
        }

        /// <summary>
        /// Procesa el pago del carrito de compras del usuario autenticado
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = $"{nameof(RoleType.Customer)},{nameof(RoleType.Seller)}")]
        [HttpPost("my-cart/checkout")]
        public async Task<IActionResult> CheckoutMyCart()
        {
            try
            {
                var (isValid, tokenUserId, _) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var payment = await _orderService.ProcessPaymentAsync(tokenUserId);
                var paymentDto = _mapper.Map<PaymentDto>(payment);
                var response = new ApiResponse<PaymentDto>(paymentDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Procesa el pago del carrito de compras del usuario
        /// </summary>
        /// <remarks>
        /// Solo para uso del Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpPost("user/{userId}/process-payment")]
        public async Task<IActionResult> ProcessPaymentAsync(int userId)
        {
            try
            {
                var payment = await _orderService.ProcessPaymentAsync(userId);
                var paymentDto = _mapper.Map<PaymentDto>(payment);
                var response = new ApiResponse<PaymentDto>(paymentDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }

        }

        /// <summary>
        /// Obtiene el historial de órdenes completadas del usuario autenticado
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<OrderResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = $"{nameof(RoleType.Customer)},{nameof(RoleType.Seller)}")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var (isValid, tokenUserId, _) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var orders = await _orderService.GetAllOderUserAsync(tokenUserId);
                var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

                var response = new ApiResponse<IEnumerable<OrderResponseDto>>(ordersDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el historial de órdenes de un usuario específico
        /// </summary>
        /// <remarks>
        /// Solo uso del Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<OrderResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("user/{userId}/orders")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = userId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID de usuario",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var orders = await _orderService.GetAllOderUserAsync(userId);
                var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

                var response = new ApiResponse<IEnumerable<OrderResponseDto>>(ordersDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene las ventas de productos del Seller autenticado
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<OrderResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [Authorize(Roles = nameof(RoleType.Seller))]
        [HttpGet("my-sales")]
        public async Task<IActionResult> GetMySales()
        {
            try
            {
                var (isValid, tokenUserId, _) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var orders = await _orderService.GetSellerSalesAsync(tokenUserId);
                var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

                var response = new ApiResponse<IEnumerable<OrderResponseDto>>(ordersDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }


        /// <summary>
        /// Obtiene el reporte mensual de ventas del sistema
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ReporteMensualVentasResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("dapper/monthly-sales")]
        public async Task<IActionResult> GetReporteMensualVentas()
        {
            var posts = await _orderService.GetReporteMensualVentas();
            var response = new ApiResponse<IEnumerable<ReporteMensualVentasResponse>> (posts);
            return Ok(response);
        }

        /// <summary>
        /// Obtiene estadísticas generales del tablero de control
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<BoardStatsResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("dapper/board-stats")]
        public async Task<IActionResult> GetBoardStats()
        {
            var posts = await _orderService.GetBoardStats();
            var response = new ApiResponse<IEnumerable<BoardStatsResponse>> (posts);
            return Ok(response);
        }

        /// <summary>
        /// Obtiene la lista de productos más vendidos en el sistema
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<TopProductosVendidosResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("dapper/top-products")]
        public async Task<IActionResult> GetTopProductosVendidos()
        {
            var posts = await _orderService.GetTopProductosVendidos();
            var response = new ApiResponse<IEnumerable<TopProductosVendidosResponse>> (posts);
            return Ok(response);
        }

        /// <summary>
        /// Obtiene la lista de productos con stock bajo
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<LowStockProductResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("dapper/low-stock")]
        public async Task<IActionResult> GetLowStockProductResponse()
        {
            var posts = await _orderService.GetLowStockProductResponse();
            var response = new ApiResponse<IEnumerable<LowStockProductResponse>> (posts);
            return Ok(response);
        }

        /// <summary>
        /// Obtiene la lista de usuarios con mayor gasto total
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator.
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<TopUsersBySpendingResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("dapper/top-users")]
        public async Task<IActionResult> GetTopUsersBySpending()
        {
            var posts = await _orderService.GetTopUsersBySpending();
            var response = new ApiResponse<IEnumerable<TopUsersBySpendingResponse>> (posts);
            return Ok(response);
        }

    }
}
