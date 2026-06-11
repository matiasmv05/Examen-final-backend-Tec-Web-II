using Amazon.Core.Entities;
using Amazon.Core.QueryFilters;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Filtra los parámetros de órdenes/pedidos
/// </summary>
/// <remarks>
/// Esta clase permite filtrar órdenes por diferentes criterios como usuario,
/// monto total, estado y fecha de actualización. También incluye paginación.
/// </remarks>
public class OrderQueryFilter : PaginationQueryFilter
{
    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    /// <example>1</example>
    [SwaggerSchema("Identificador único del usuario que realizó la orden")]
    public int UserId { get; set; }

    /// <summary>
    /// Monto total de la orden
    /// </summary>
    /// <example>1250.50</example>
    [SwaggerSchema("Monto total de la orden. Permite filtrar por órdenes de un monto específico")]
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Estado actual de la orden
    /// </summary>
    /// <example>Paid</example>
    [SwaggerSchema("Estado actual de la orden. Valores posibles: Cart, Pending, Paid")]
    public string? Status { get; set; }

    /// <summary>
    /// Fecha de última actualización de la orden
    /// </summary>
    /// <example>2025-11-19T21:04:02.24</example>
    [SwaggerSchema("Fecha y hora de la última actualización de la orden")]
    public DateTime? UpdatedAt { get; set; }
}
