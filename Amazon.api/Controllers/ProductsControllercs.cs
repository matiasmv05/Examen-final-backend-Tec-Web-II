using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
using Amazon.Core.Services;
using Amazon.infrastructure.DTOs;
using Amazon.Infrastructure.Repositories;
using Amazon.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Amazon.api.Controllers
{
    [Authorize] // Autenticación base para todos los roles
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public ProductsController(
            IValidationService validationService,
            IMapper mapper,
            IProductService productService,
            IUserService userService)
        {
            _mapper = mapper;
            _validationService = validationService;
            _productService = productService;
            _userService = userService;
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
        /// Obtiene una lista paginada de productos con filtros opcionales
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera una lista de productos con soporte para paginación y filtrado.
        /// Los resultados incluyen información básica como nombre, precio, categoría y stock.
        /// Accesible para todos los roles autenticados.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <param name="productQueryFilter">Filtros de búsqueda y paginación para productos</param>
        /// <returns>Lista paginada de productos que coinciden con los criterios</returns>
        /// <response code="200">Retorna la lista de productos exitosamente</response>
        /// <response code="400">Error en la validación de los parámetros de filtrado</response>
        /// <response code="401">No autenticado. Token JWT requerido</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryFilter productQueryFilter)
        {
            try
            {
                var products = await _productService.GetAllProducts(productQueryFilter);

                var productDto = _mapper.Map<IEnumerable<ProductDto>>(products.Pagination);

                var pagination = new Pagination
                {
                    TotalCount = products.Pagination.TotalCount,
                    PageSize = products.Pagination.PageSize,
                    CurrentPage = products.Pagination.CurrentPage,
                    TotalPages = products.Pagination.TotalPages,
                    HasNextPage = products.Pagination.HasNextPage,
                    HasPreviousPage = products.Pagination.HasPreviousPage
                };
                var response = new ApiResponse<IEnumerable<ProductDto>>(productDto)
                {
                    Pagination = pagination,
                    Messages = products.Messages
                };

                return StatusCode((int)products.StatusCode, response);
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
        /// Obtiene un producto específico por su identificador único
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera la información completa de un producto utilizando su ID único.
        /// Incluye detalles como descripción, precio, stock, categoría e información del vendedor.
        /// Accesible para todos los roles autenticados.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/5
        /// </remarks>
        /// <param name="id">Identificador único del producto (mayor a 0)</param>
        /// <returns>Información detallada del producto solicitado</returns>
        /// <response code="200">Retorna el producto encontrado exitosamente</response>
        /// <response code="400">Error en la validación del ID del producto</response>
        /// <response code="404">No se encontró el producto con el ID especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetbyId(int id)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = id };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                var productDto = _mapper.Map<ProductDto>(product);
                var response = new ApiResponse<ProductDto>(productDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Crea un nuevo producto en el sistema
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator y Seller.
        /// El SellerId se asigna automáticamente desde el token JWT.
        /// El Administrator puede opcionalmente especificar un SellerId diferente mediante query parameter.
        /// 
        /// Ejemplo de body:
        /// {
        ///   "name": "Laptop Gaming",
        ///   "description": "Laptop para gaming de alta performance",
        ///   "price": 1500.00,
        ///   "stock": 10,
        ///   "category": "Electrónicos"
        /// }
        /// </remarks>
        /// <param name="createProductDto">Datos del producto a crear (sin Id ni SellerId)</param>
        /// <param name="sellerId">Opcional. Solo para Administrator: asigna el producto a un vendedor específico</param>
        /// <returns>Producto creado con su identificador asignado</returns>
        /// <response code="200">Producto creado exitosamente</response>
        /// <response code="400">Error en la validación de datos o vendedor no existe</response>
        /// <response code="403">No autorizado. Se requiere rol Administrator o Seller</response>
        /// <response code="500">Error interno del servidor al crear el producto</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.Seller)}")]
        [HttpPost]
        public async Task<IActionResult> InsertProduct([FromBody] CreateProductDto createProductDto, [FromQuery] int? sellerId = null)
        {
            try
            {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                #region Validaciones
                var validationResult = await _validationService.ValidateAsync(createProductDto);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                // Determinar el SellerId
                int resolvedSellerId;
                if (userRole == nameof(RoleType.Administrator) && sellerId.HasValue)
                {
                    // Administrator puede asignar a otro vendedor
                    var seller = await _userService.GetByIdAsync(sellerId.Value);
                    if (seller == null)
                        return BadRequest(new ApiResponse<string>("El vendedor especificado no existe"));
                    resolvedSellerId = sellerId.Value;
                }
                else
                {
                    // Seller siempre usa su propio Id; Admin sin sellerId también usa el suyo
                    resolvedSellerId = tokenUserId;
                }

                var product = _mapper.Map<Product>(createProductDto);
                product.SellerId = resolvedSellerId;

                await _productService.AddAsync(product);

                var createdProduct = await _productService.GetByIdAsync(product.Id);
                var productDtoResponse = _mapper.Map<ProductDto>(createdProduct);

                var response = new ApiResponse<ProductDto>(productDtoResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error al crear producto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza la información de un producto existente
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator y Seller.
        /// El Seller solo puede actualizar sus propios productos.
        /// El Administrator puede actualizar cualquier producto.
        /// 
        /// Ejemplo de uso:
        /// PUT /api/Products/5
        /// </remarks>
        /// <param name="id">Identificador único del producto a actualizar</param>
        /// <param name="productDto">Nuevos datos del producto</param>
        /// <returns>Producto actualizado con la nueva información</returns>
        /// <response code="200">Producto actualizado exitosamente</response>
        /// <response code="400">Error de validación o IDs no coinciden</response>
        /// <response code="403">No autorizado. El Seller solo puede editar sus propios productos</response>
        /// <response code="404">No se encontró el producto a actualizar</response>
        /// <response code="500">Error interno del servidor al actualizar</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.Seller)}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            try
            {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                if (id != productDto.Id)
                    return BadRequest(new ApiResponse<string>("El Id del Producto no coincide"));

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                // Seller solo puede editar sus propios productos
                if (userRole == nameof(RoleType.Seller) && product.SellerId != tokenUserId)
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new ApiResponse<string>("No tienes permiso para editar este producto"));

                #region Validaciones
                var validationResult = await _validationService.ValidateAsync(productDto);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                _mapper.Map(productDto, product);
                await _productService.Update(product);
                var updatedProduct = await _productService.GetByIdAsync(id);
                var productDtoResponse = _mapper.Map<ProductDto>(updatedProduct);

                var response = new ApiResponse<ProductDto>(productDtoResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error al actualizar producto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Elimina un producto del sistema
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator y Seller.
        /// El Seller solo puede eliminar sus propios productos.
        /// El Administrator puede eliminar cualquier producto.
        /// 
        /// Ejemplo de uso:
        /// DELETE /api/Products/5
        /// </remarks>
        /// <param name="id">Identificador único del producto a eliminar</param>
        /// <returns>Respuesta sin contenido (204) si la eliminación fue exitosa</returns>
        /// <response code="204">Producto eliminado exitosamente</response>
        /// <response code="403">No autorizado. El Seller solo puede eliminar sus propios productos</response>
        /// <response code="404">No se encontró el producto a eliminar</response>
        /// <response code="500">Error interno del servidor al eliminar</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.Seller)}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductDtoMapper(int id)
        {
            try
            {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                // Seller solo puede eliminar sus propios productos
                if (userRole == nameof(RoleType.Seller) && product.SellerId != tokenUserId)
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new ApiResponse<string>("No tienes permiso para eliminar este producto"));

                await _productService.Delete(product.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error al eliminar producto: {ex.Message}"));
            }
        }


        /// <summary>
        /// Obtiene productos filtrados por categoría específica
        /// </summary>
        /// <remarks>
        /// Accesible para todos los roles autenticados.
        /// Útil para filtrar productos por tipo (Electrónicos, Ropa, Hogar, etc.).
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/category/Electrónicos
        /// </remarks>
        /// <param name="category">Nombre de la categoría a filtrar</param>
        /// <returns>Lista de productos que pertenecen a la categoría especificada</returns>
        /// <response code="200">Retorna los productos de la categoría exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductDto>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            try
            {
                var products = await _productService.GetByCategoryAsync(category);
                var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
                var response = new ApiResponse<IEnumerable<ProductDto>>(productsDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene todos los productos de un vendedor específico
        /// </summary>
        /// <remarks>
        /// Accesible para todos los roles autenticados.
        /// Útil para ver todos los productos que un vendedor tiene registrados en el sistema.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/seller/1
        /// </remarks>
        /// <param name="sellerId">Identificador único del vendedor</param>
        /// <returns>Lista de productos asociados al vendedor especificado</returns>
        /// <response code="200">Retorna los productos del vendedor exitosamente</response>
        /// <response code="400">Error en la validación del ID del vendedor</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetProductsBySeller(int sellerId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = sellerId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID del vendedor",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var products = await _productService.GetBySellerAsync(sellerId);
                var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
                var response = new ApiResponse<IEnumerable<ProductDto>>(productsDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza el stock de un producto específico
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administrator y Seller.
        /// El Seller solo puede actualizar el stock de sus propios productos.
        /// El Administrator puede actualizar el stock de cualquier producto.
        /// 
        /// Ejemplo de uso:
        /// PATCH /api/Products/5/stock/25
        /// </remarks>
        /// <param name="id">Identificador único del producto</param>
        /// <param name="quantity">Nueva cantidad en stock (debe ser mayor a 0)</param>
        /// <returns>Producto con el stock actualizado</returns>
        /// <response code="200">Stock actualizado exitosamente</response>
        /// <response code="400">Error de validación o cantidad inválida</response>
        /// <response code="403">No autorizado. El Seller solo puede actualizar stock de sus propios productos</response>
        /// <response code="404">No se encontró el producto</response>
        /// <response code="500">Error interno del servidor al actualizar</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.Seller)}")]
        [HttpPatch("{id}/stock/{quantity}")]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            try
            {
                var (isValid, tokenUserId, userRole) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = id };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                // Seller solo puede actualizar stock de sus propios productos
                if (userRole == nameof(RoleType.Seller) && product.SellerId != tokenUserId)
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new ApiResponse<string>("No tienes permiso para actualizar el stock de este producto"));

                product.Stock = quantity;
                await _productService.Update(product);

                var productDto = _mapper.Map<ProductDto>(product);
                var response = new ApiResponse<ProductDto>(productDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene los productos del vendedor autenticado
        /// </summary>
        /// <remarks>
        /// Solo accesible para Sellers. Extrae el SellerId del token JWT automáticamente.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/my-products
        /// </remarks>
        /// <returns>Lista de productos del vendedor autenticado</returns>
        /// <response code="200">Retorna los productos del vendedor exitosamente</response>
        /// <response code="401">No autenticado o token inválido</response>
        /// <response code="403">No autorizado. Se requiere rol Seller</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductDto>>))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Seller))]
        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            try
            {
                var (isValid, tokenUserId, _) = GetTokenClaims();
                if (!isValid)
                    return Unauthorized(new ApiResponse<string>("Token inválido"));

                var products = await _productService.GetBySellerAsync(tokenUserId);
                var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
                var response = new ApiResponse<IEnumerable<ProductDto>>(productsDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar el funcionamiento del controlador
        /// </summary>
        /// <remarks>
        /// Este endpoint permite verificar que el controlador de productos esté funcionando correctamente.
        /// No realiza operaciones sobre la base de datos, solo retorna un mensaje de confirmación.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/test
        /// </remarks>
        /// <returns>Mensaje de confirmación del funcionamiento</returns>
        /// <response code="200">API funcionando correctamente</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<string>))]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new ApiResponse<string>("Products API funcionando correctamente"));
        }

    }
}