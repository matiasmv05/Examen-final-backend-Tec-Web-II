using Amazon.Core.Entities;
using Amazon.Core.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Amazon.Core.Enum;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Amazon.Infrastructure.DTOs;

namespace Amazon.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISecurityServices _securityServices;
        private readonly IPasswordService _passwordService;
        public TokenController(IConfiguration configuration,
            ISecurityServices securityServices,
            IPasswordService passwordService)
        {
            _configuration = configuration;
            _securityServices = securityServices;
            _passwordService = passwordService;
        }

        /// <summary>
        /// Autentica un usuario y genera un token JWT
        /// </summary>
        /// <remarks>
        /// Este endpoint valida las credenciales del usuario y genera un token JWT
        /// que puede ser usado para autenticar solicitudes en endpoints protegidos.
        /// El token expira en 60 minutos.
        /// 
        /// Ejemplo de body:
        /// {
        ///     "user": "usuario123",
        ///     "password": "Contraseña123"
        /// }
        /// 
        /// La respuesta incluye un objeto con la propiedad "token" que contiene el JWT.
        /// Este token debe incluirse en el header Authorization de solicitudes protegidas:
        /// Authorization: Bearer [token]
        /// </remarks>
        /// <param name="userLogin">Credenciales de autenticación (login y password)</param>
        /// <returns>Token JWT para autenticación en endpoints protegidos</returns>
        /// <response code="200">Autenticación exitosa, retorna token JWT</response>
        /// <response code="400">Error en la validación de las credenciales</response>
        /// <response code="401">Credenciales inválidas o usuario no encontrado</response>
        /// <response code="500">Error interno del servidor al procesar la autenticación</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]

        [HttpPost]
        public async Task<IActionResult> Authentication([FromBody] UserLogin userLogin)
        {
            try
            {
                //Si es usuario valido
                var validation = await IsValidUser(userLogin);
                if (validation.Item1)
                {
                    var security = validation.Item2;
                    var token = GenerateToken(security);
                    
                    var response = new Amazon.Infrastructure.DTOs.LoginResponseDto 
                    {
                        Token = token,
                        UserId = security.UserId,
                        Name = security.Name,
                        Email = security.User?.Email ?? string.Empty,
                        Role = security.Role,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                    };

                    return Ok(response);
                }

                return NotFound("Credenciales no válidas");
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }


        private async Task<(bool, Security)> IsValidUser(UserLogin userLogin)
{
    var user = await _securityServices.GetLoginByCredentials(userLogin);

    // ¿Tienes esta línea? Si no, ese es el problema
    if (user == null)
        return (false, null!);

    var isValidHash = _passwordService.Check(user.Password, userLogin.Password);
    return (isValidHash, user);
}

        private string GenerateToken(Security security)
{
    string secretKey = _configuration["Authentication:SecretKey"];

    var symmetricSecurityKey =
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var signingCredentials =
        new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
    var header = new JwtHeader(signingCredentials);

    var claims = new[]
    {
        // CAMBIO: security.UserId en lugar de security.Id
        new Claim(ClaimTypes.NameIdentifier, security.UserId.ToString()),
        new Claim("Email", security.User?.Email ?? string.Empty),
        new Claim("Name", security.Name),
        new Claim(ClaimTypes.Role, security.Role.ToString())
    };

    var payload = new JwtPayload(
        issuer: _configuration["Authentication:Issuer"],
        audience: _configuration["Authentication:Audience"],
        claims: claims,
        notBefore: DateTime.UtcNow,
        expires: DateTime.UtcNow.AddMinutes(60)
    );

    var token = new JwtSecurityToken(header, payload);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

        /// <summary>
        /// Obtiene información de configuración del servidor (solo desarrollo)
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna información sensible de configuración como cadenas de conexión
        /// y configuraciones de autenticación. **DEBE ESTAR PROTEGIDO EN PRODUCCIÓN**.
        /// 
        /// Útil para debugging en entornos de desarrollo.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Token/Config
        /// </remarks>
        /// <returns>Información de configuración del servidor</returns>
        /// <response code="200">Retorna la información de configuración</response>
        /// <response code="500">Error interno del servidor al obtener la configuración</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(object))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [HttpGet("Config")]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                var connectionStringSqlServer = _configuration["ConnectionStrings:ConnectionSqlServer"];

                var result = new
                {
                    connectionStringSqlServer = connectionStringSqlServer ?? "SQL SERVER NO CONFIGURADO",
                    AllConnectionStrings = _configuration.GetSection("ConnectionStrings").GetChildren().Select(x => new { Key = x.Key, Value = x.Value }),
                    Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "ASPNETCORE_ENVIRONMENT NO CONFIGURADO",
                    Authentication = _configuration.GetSection("Authentication").GetChildren().Select(x => new { Key = x.Key, Value = x.Value })
                };

                return Ok(result);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }
    }
}
