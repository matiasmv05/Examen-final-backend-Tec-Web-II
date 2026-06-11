using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
using Amazon.Core.Services;
using Amazon.infrastructure.DTOs;
using Amazon.Infrastructure.DTOs;
using Amazon.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;


namespace Amazon.api.Controllers
{

    [Authorize] // Autenticación base para todos
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly IOrderService _orderService;

        public PaymentsController(
            IPaymentService paymentService,
            IMapper mapper,
            IValidationService validationService,
            IOrderService orderService)
        {
            _paymentService = paymentService;
            _mapper = mapper;
            _validationService = validationService;
            _orderService = orderService;
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
        /// Obtiene una lista paginada de pagos con filtros opcionales
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Payments?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <param name="paymentQueryFilter">Filtros de búsqueda y paginación para pagos</param>
        /// <returns>Lista paginada de pagos que coinciden con los criterios</returns>
        /// <response code="200">Retorna la lista de pagos exitosamente</response>
        /// <response code="400">Error en la validación de los parámetros de filtrado</response>
        /// <response code="401">No autenticado. Token JWT requerido</response>
        /// <response code="403">No autorizado. Se requiere rol Administrator</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<PaymentResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] PaymentQueryFilter paymentQueryFilter)
        {
            try
            {
                var payments = await _paymentService.GetAllPayments(paymentQueryFilter);

                var paymentsDto = _mapper.Map<IEnumerable<PaymentResponseDto>>(payments.Pagination);

                var pagination = new Pagination
                {
                    TotalCount = payments.Pagination.TotalCount,
                    PageSize = payments.Pagination.PageSize,
                    CurrentPage = payments.Pagination.CurrentPage,
                    TotalPages = payments.Pagination.TotalPages,
                    HasNextPage = payments.Pagination.HasNextPage,
                    HasPreviousPage = payments.Pagination.HasPreviousPage
                };
                var response = new ApiResponse<IEnumerable<PaymentResponseDto>>(paymentsDto)
                {
                    Pagination = pagination,
                    Messages = payments.Messages
                };

                return StatusCode((int)payments.StatusCode, response);
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
        /// Obtiene un pago específico por su identificador único
        /// </summary>
        /// <remarks>
        /// El Administrator puede ver cualquier pago.
        /// Customer/Seller solo pueden ver pagos de órdenes que les pertenecen.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Payments/1
        /// </remarks>
        /// <param name="id">Identificador único del pago (mayor a 0)</param>
        /// <returns>Información detallada del pago solicitado</returns>
        /// <response code="200">Retorna el pago encontrado exitosamente</response>
        /// <response code="400">Error en la validación del ID del pago</response>
        /// <response code="403">No autorizado para ver este pago</response>
        /// <response code="404">No se encontró el pago con el ID especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                    return NotFound(new ApiResponse<string>("Pago no encontrado"));

                // Verificar que el pago pertenezca al usuario si no es admin
                if (userRole != nameof(RoleType.Administrator))
                {
                    var order = await _orderService.GetByIdOrderAsync(payment.OrderId.GetValueOrDefault());
                    if (order == null || order.UserId != tokenUserId)
                    {
                         return StatusCode((int)HttpStatusCode.Forbidden,
                             new ApiResponse<string>("No tienes permiso para ver este pago"));
                    }
                }
                
                var paymentDto = _mapper.Map<PaymentResponseDto>(payment);
                var response = new ApiResponse<PaymentResponseDto>(paymentDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el pago asociado a una orden específica
        /// </summary>
        /// <remarks>
        /// El Administrator puede ver cualquier pago.
        /// Customer/Seller solo pueden ver pagos de órdenes que les pertenecen.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Payments/order/5
        /// </remarks>
        /// <param name="orderId">Identificador único de la orden (mayor a 0)</param>
        /// <returns>Información del pago asociado a la orden</returns>
        /// <response code="200">Retorna el pago asociado a la orden exitosamente</response>
        /// <response code="400">Error en la validación del ID de la orden</response>
        /// <response code="403">No autorizado para ver este pago</response>
        /// <response code="404">No se encontró pago para la orden especificada</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            try
            { 
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                if (userRole != nameof(RoleType.Administrator))
                {
                    var order = await _orderService.GetByIdOrderAsync(orderId);
                    if (order == null || order.UserId != tokenUserId)
                    {
                         return StatusCode((int)HttpStatusCode.Forbidden,
                             new ApiResponse<string>("No tienes permiso para ver pagos de esta orden"));
                    }
                }

                var payment = await _paymentService.GetByOrderIdAsync(orderId);
                if (payment == null)
                    return NotFound(new ApiResponse<string>("Pago no encontrado"));

                var paymentDto = _mapper.Map<PaymentResponseDto>(payment);
                var response = new ApiResponse<PaymentResponseDto>(paymentDto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene todos los pagos del usuario autenticado
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<PaymentResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = $"{nameof(RoleType.Customer)},{nameof(RoleType.Seller)}")]
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var (isValid, tokenUserId, _) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var allPayments = await _paymentService.GetAllAsync();
                
                // Filtrar los pagos que pertenecen a las órdenes del usuario
                var userPayments = allPayments.Where(p => p.Order?.UserId == tokenUserId);
                
                var paymentsDto = _mapper.Map<IEnumerable<PaymentResponseDto>>(userPayments);
                var response = new ApiResponse<IEnumerable<PaymentResponseDto>>(paymentsDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                 return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }
    }
  }
