using Amazon.infrastructure.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Validators
{
    public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("El nombre del producto es requerido")
                .MaximumLength(200).WithMessage("El nombre del producto no puede exceder 200 caracteres");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
                .LessThan(1000).WithMessage("La cantidad no puede ser mayor a 1000");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0");

            RuleFor(x => x.Subtotal)
                .GreaterThanOrEqualTo(0).WithMessage("El subtotal debe ser mayor o igual a 0");
        }
    }
}
