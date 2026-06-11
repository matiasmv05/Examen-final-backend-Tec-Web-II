using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.QueryFilters
{
    /// <summary>
    /// Filtra los parámetros de usuarios
    /// </summary>
    public class UserQueryFilter : PaginationQueryFilter
    {
        /// <summary>
        /// Nombre del usuario
        /// </summary>
        /// <example>Juan Pérez</example>
        [SwaggerSchema("Nombre del usuario")]
        public string Name { get; set; }

        /// <summary>
        /// Email del usuario
        /// </summary>
        /// <example>juan.perez@email.com</example>
        [SwaggerSchema("Email del usuario")]
        public string Email { get; set; }
    }
}
