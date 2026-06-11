using Amazon.api.Responses;
using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Amazon.Infrastructure.DTOs;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Amazon.api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityServices _securityServices;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        public SecurityController(ISecurityServices securityServices,
            IMapper mapper,
            IPasswordService passwordService,
            IUserService userService) 
        {
            _securityServices = securityServices;
            _mapper = mapper;
            _passwordService = passwordService;
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
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint permite registrar un nuevo usuario en el sistema Amazon.
        /// La contraseña se hashea automáticamente antes de almacenarse.
        /// 
        /// Notas de seguridad:
        /// - El rol "Administrator" solo puede ser asignado si la petición la hace un Administrator autenticado.
        /// - En el registro público, si no se especifica rol o se intenta usar "Administrator", se asigna "Customer".
        /// 
        /// Ejemplo de body:
        /// {
        ///     "email": "juan@example.com",
        ///     "password": "ContraseñaSegura123!",
        ///     "name": "Juan Pérez",
        ///     "role": 2 // 1: Seller, 2: Customer
        /// }
        /// </remarks>
        /// <param name="securityDto">Datos del usuario a registrar</param>
        /// <returns>Usuario registrado con su identificador asignado</returns>
        /// <response code="200">Usuario registrado exitosamente</response>
        /// <response code="400">Error en la validación de datos o usuario ya existe</response>
        /// <response code="403">Intento de crear un administrador sin permisos</response>
        /// <response code="500">Error interno del servidor al registrar el usuario</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RegisterResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType((int)HttpStatusCode.Forbidden, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpPost("register")]
        public async Task<IActionResult> Register(SecurityDto securityDto)
        {
            var (isValid, _, userRole) = GetTokenClaims();

           
            if (securityDto.Role == RoleType.Administrator)
            {
                // Solo un admin autenticado puede crear otro admin
                if (!isValid || userRole != nameof(RoleType.Administrator))
                {
                     return StatusCode((int)HttpStatusCode.Forbidden,
                        new ApiResponse<string>("No tienes permiso para registrar un usuario Administrator"));
                }
            }
            
            else if (!securityDto.Role.HasValue)
            {
                // Por defecto Customer
                securityDto.Role = RoleType.Customer;
            }
            // Si el rol es Seller o Customer, se permite el registro público

            var security = _mapper.Map<Security>(securityDto);
            security.Password = _passwordService.Hash(securityDto.Password);

            await _securityServices.RegisterUser(security);

            var registerResponseDto = _mapper.Map<RegisterResponseDto>(security);

            var response = new ApiResponse<RegisterResponseDto>(registerResponseDto);
            return Ok(response);
        }


    }
}
