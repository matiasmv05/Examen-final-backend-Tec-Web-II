using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.QueryFilters
{
    /// <summary>
    /// Filtra los parámetros de productos
    /// </summary>
    /// <remarks>
    /// Esta clase permite filtrar productos por diferentes criterios como vendedor,
    /// nombre, precio y categoría. Incluye paginación automática.
    /// 
    /// Ejemplo de uso en URL:
    /// ?SellerId=1&Category=Electrónicos&PageSize=20&PageNumber=1
    /// </remarks>
    public class ProductQueryFilter : PaginationQueryFilter
    {
        /// <summary>
        /// Identificador único del vendedor
        /// </summary>
        /// <example>1</example>
        [SwaggerSchema("Identificador único del vendedor")]
        public int SellerId { get; set; }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        /// <example>Laptop</example>
        [SwaggerSchema("Nombre del producto")]
        public string Name { get; set; }

        /// <summary>
        /// Precio del producto
        /// </summary>
        /// <example>1200.00</example>
        [SwaggerSchema("Precio del producto")]
        public decimal Price { get; set; }

        /// <summary>
        /// Categoría del producto
        /// </summary>
        /// <example>Electrónicos</example>
        [SwaggerSchema("Categoría del producto")]
        public string Category { get; set; }

        /// <summary>
        /// Detalles del producto
        /// </summary>
        /// <example>En buen estado</example>
        [SwaggerSchema("Descripcion del producto")]
        public string Description { get; set; }
    }
}
