using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
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
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly ISecurityServices _securityServices;

        public UsersController(IUserService userService, IMapper mapper, IValidationService validationService,ISecurityServices securityServices)
        {
            _mapper = mapper;
            _validationService = validationService;
            _userService = userService;
             _securityServices = securityServices;
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
        /// Obtiene una lista paginada de todos los usuarios
        /// </summary>
        /// <remarks>
        /// Solo accesible para Administradores.
        /// Ejemplo: GET /api/Users?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Lista de usuarios recuperada exitosamente</response>
        /// <response code="401">No autenticado</response>
        /// <response code="403">No autorizado. Se requiere rol Administrator</response>
        /// <response code="500">Error interno del servidor</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<UserProfileDto>>))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserQueryFilter userQueryFilter)
        {
            var users = await _userService.GetAllUsers(userQueryFilter);
            var usersDto = _mapper.Map<IEnumerable<UserProfileDto>>(users.Pagination);
            var pagination = new Pagination
            {
                TotalCount = users.Pagination.TotalCount,
                PageSize = users.Pagination.PageSize,
                CurrentPage = users.Pagination.CurrentPage,
                TotalPages = users.Pagination.TotalPages,
                HasNextPage = users.Pagination.HasNextPage,
                HasPreviousPage = users.Pagination.HasPreviousPage
            };
            var response = new ApiResponse<IEnumerable<UserProfileDto>>(usersDto)
            {
                Pagination = pagination,
                Messages = users.Messages
            };
            return StatusCode((int)users.StatusCode, response);
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <remarks>
        /// El Administrador puede ver cualquier usuario.
        /// Un usuario autenticado solo puede ver su propio perfil.
        /// </remarks>
        /// <param name="id">ID del usuario</param>
        /// <response code="200">Usuario encontrado exitosamente</response>
        /// <response code="400">ID inválido</response>
        /// <response code="401">No autenticado</response>
        /// <response code="403">No autorizado para ver este usuario</response>
        /// <response code="404">Usuario no encontrado</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<UserProfileDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var (isValid, tokenUserId, userRole) = GetTokenClaims();
            if (!isValid)
                return Unauthorized(new ApiResponse<string>("Token inválido"));

            if (userRole != nameof(RoleType.Administrator) && tokenUserId != id)
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new ApiResponse<string>("No tienes permiso para ver este usuario"));

            var validationRequest = new GetByIdRequest { Id = id };
            var validationResult = await _validationService.ValidateAsync(validationRequest);
            if (!validationResult.IsValid)
                return BadRequest(new { Errors = validationResult.Errors });

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse<string>("Usuario no encontrado"));

            return Ok(new ApiResponse<UserProfileDto>(_mapper.Map<UserProfileDto>(user)));
        }

        /// <summary>
        /// Actualiza la información de un usuario
        /// </summary>
        /// <remarks>
        /// El Administrador puede actualizar cualquier usuario.
        /// Un usuario autenticado solo puede actualizar su propio perfil.
        /// </remarks>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="userDto">Nuevos datos del usuario</param>
        /// <response code="200">Usuario actualizado exitosamente</response>
        /// <response code="400">Error de validación</response>
        /// <response code="401">No autenticado</response>
        /// <response code="403">No autorizado para editar este usuario</response>
        /// <response code="404">Usuario no encontrado</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<UserProfileDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
{
    var (isValid, tokenUserId, userRole) = GetTokenClaims();
    if (!isValid)
        return Unauthorized(new ApiResponse<string>("Token inválido"));

    if (userRole != nameof(RoleType.Administrator) && tokenUserId != id)
        return StatusCode((int)HttpStatusCode.Forbidden,
            new ApiResponse<string>("No tienes permiso para editar este usuario"));

    var user = await _userService.GetByIdAsync(id);
    if (user == null)
        return NotFound(new ApiResponse<string>("Usuario no encontrado"));

    // Solo mapear Name y Email — IsActive no se toca
    user.Name = userDto.Name;
    user.Email = userDto.Email;

    await _userService.Update(user);
    return Ok(new ApiResponse<UserDto>(_mapper.Map<UserDto>(user)));
}

/// <summary>
/// Actualiza completamente un usuario (solo Administrator)
/// </summary>
/// <remarks>
/// Permite al Administrador modificar nombre, email, estado activo y rol de cualquier usuario.
/// </remarks>
/// <param name="id">ID del usuario a actualizar</param>
/// <param name="adminDto">Datos completos a actualizar</param>
/// <response code="200">Usuario actualizado exitosamente</response>
/// <response code="404">Usuario no encontrado</response>
/// <response code="403">No autorizado. Se requiere rol Administrator</response>
[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<UserDto>))]
[ProducesResponseType((int)HttpStatusCode.NotFound)]
[ProducesResponseType((int)HttpStatusCode.Forbidden)]
[Authorize(Roles = nameof(RoleType.Administrator))]
[HttpPut("{id}/admin")]
public async Task<IActionResult> UpdateUserAdmin(int id, [FromBody] UpdateUserAdminDto adminDto)
{
    var user = await _userService.GetByIdAsync(id);
    if (user == null)
        return NotFound(new ApiResponse<string>("Usuario no encontrado"));

    user.Name = adminDto.Name;
    user.Email = adminDto.Email;
    user.IsActive = adminDto.IsActive;

    if (adminDto.Role.HasValue)
        await _securityServices.UpdateRoleAsync(id, adminDto.Role.Value);

    await _userService.Update(user);
    return Ok(new ApiResponse<UserDto>(_mapper.Map<UserDto>(user)));
}

        /// <summary>
        /// Elimina un usuario del sistema
        /// </summary>
        /// <remarks>
        /// El Administrador puede eliminar cualquier usuario.
        /// Un usuario autenticado solo puede eliminar su propia cuenta.
        /// </remarks>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <response code="204">Usuario eliminado exitosamente</response>
        /// <response code="401">No autenticado</response>
        /// <response code="403">No autorizado para eliminar este usuario</response>
        /// <response code="404">Usuario no encontrado</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var (isValid, tokenUserId, userRole) = GetTokenClaims();
            if (!isValid)
                return Unauthorized(new ApiResponse<string>("Token inválido"));

            if (userRole != nameof(RoleType.Administrator) && tokenUserId != id)
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new ApiResponse<string>("No tienes permiso para eliminar esta cuenta"));

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse<string>("Usuario no encontrado"));

            await _userService.Delete(user.Id);
            return NoContent();
        }

        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateUser(int id)
        {
           var user = await _userService.ActivarUsuario(id);
           UserProfileDto userDto = _mapper.Map<UserProfileDto>(user);
           return Ok(new ApiResponse<UserProfileDto>(userDto));
            
        }

        /// <summary>
        /// Obtiene el saldo de billetera de un usuario
        /// </summary>
        /// <remarks>
        /// El Administrador puede ver la billetera de cualquier usuario.
        /// Un usuario autenticado solo puede ver su propia billetera.
        /// </remarks>
        /// <param name="id">ID del usuario</param>
        /// <response code="200">Saldo recuperado exitosamente</response>
        /// <response code="401">No autenticado</response>
        /// <response code="403">No autorizado para ver esta billetera</response>
        /// <response code="404">Usuario no encontrado</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(WalletResponseDto))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpGet("{id}/billetera")]
        public async Task<IActionResult> GetUserBalance(int id)
        {
            var (isValid, tokenUserId, userRole) = GetTokenClaims();
            if (!isValid)
                return Unauthorized(new ApiResponse<string>("Token inválido"));

            if (userRole != nameof(RoleType.Administrator) && tokenUserId != id)
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new ApiResponse<string>("No tienes permiso para ver esta billetera"));

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse<string>("Usuario no encontrado"));

            return Ok(new WalletResponseDto { UserId = user.Id, Billetera = user.Billetera });
        }

       /// <summary>
       /// Recarga saldo en la billetera de un usuario
       /// </summary>
       /// <remarks>
       /// El Administrador puede recargar la billetera de cualquier usuario.
       /// Un usuario autenticado solo puede recargar su propia billetera.
       /// </remarks>
       /// <param name="id">ID del usuario</param>
       /// <param name="amount">Monto a recargar</param>
       /// <response code="200">Billetera recargada exitosamente</response>
       /// <response code="401">No autenticado</response>
       /// <response code="403">No autorizado para recargar esta billetera</response>
       /// <response code="404">Usuario no encontrado</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<WalletResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPost("{id}/billetera/llenar/{amount}")]
        public async Task<IActionResult> Wallet(int id, decimal amount)
        {
           var (isValid, tokenUserId, userRole) = GetTokenClaims();
           if (!isValid)
             return Unauthorized(new ApiResponse<string>("Token inválido"));

           if (userRole != nameof(RoleType.Administrator) && tokenUserId != id)
               return StatusCode((int)HttpStatusCode.Forbidden,
               new ApiResponse<string>("No tienes permiso para recargar esta billetera"));

         var user = await _userService.GetByIdAsync(id);
         if (user == null)
            return NotFound(new ApiResponse<string>("Usuario no encontrado"));

         await _userService.UpdateWalletAsync(id, amount);

         var updatedUser = await _userService.GetByIdAsync(id);
         var walletResponse = new WalletResponseDto { UserId = updatedUser.Id, Billetera = updatedUser.Billetera };
         return Ok(new ApiResponse<WalletResponseDto>(walletResponse));
        }

/// <summary>
/// Cambia el rol de un usuario entre Customer y Seller
/// </summary>
/// <remarks>
/// Solo el Administrador puede cambiar roles.
/// Solo se permite cambiar entre Customer y Seller.
/// No se puede asignar ni quitar el rol Administrator.
/// </remarks>
/// <param name="id">ID del usuario</param>
/// <param name="roleDto">Nuevo rol a asignar</param>
/// <response code="200">Rol actualizado exitosamente</response>
/// <response code="400">Rol no permitido</response>
/// <response code="404">Usuario no encontrado</response>
[ProducesResponseType((int)HttpStatusCode.OK)]
[ProducesResponseType((int)HttpStatusCode.BadRequest)]
[ProducesResponseType((int)HttpStatusCode.NotFound)]
[Authorize(Roles = nameof(RoleType.Administrator))]
[HttpPatch("{id}/role")]
public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto roleDto)
{
    if (roleDto.Role != RoleType.Customer && roleDto.Role != RoleType.Seller)
        return BadRequest(new ApiResponse<string>(
            "Solo se puede asignar el rol Customer o Seller"));

    var user = await _userService.GetByIdAsync(id);
    if (user == null)
        return NotFound(new ApiResponse<string>("Usuario no encontrado"));

    await _securityServices.UpdateRoleAsync(id, roleDto.Role);
    return Ok(new ApiResponse<string>($"Rol actualizado a {roleDto.Role}"));
}
    }
}