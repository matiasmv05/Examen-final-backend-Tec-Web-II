using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.QueryFilters
{
    /// <summary>
    /// Clase base para filtros con soporte de paginación
    /// </summary>
    /// <remarks>
    /// Esta clase abstracta proporciona propiedades estándar para implementar
    /// paginación en consultas de API. Todas las clases de filtro que requieran
    /// paginación deben heredar de esta clase.
    /// 
    /// Ejemplo de uso en URL:
    /// ?PageSize=20&PageNumber=2
    /// </remarks>
    public abstract class PaginationQueryFilter
    {
        /// <summary>
        /// Cantidad de elementos por página
        /// </summary>
        /// <example>10</example>
        [SwaggerSchema("Cantidad de elementos por página")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Número de página a consultar
        /// </summary>
        /// <example>1</example>
        [SwaggerSchema("Número de página a consultar")]
        public int PageNumber { get; set; } = 1;
    }
}
