using Amazon.Core.CustomEntities;
using Amazon.infrastructure.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Validators
{
    public class OrderItemRequestValidator: AbstractValidator<OrderItemRequest>
    {
        public OrderItemRequestValidator() {
            RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
                    .LessThan(1000).WithMessage("La cantidad no puede ser mayor a 1000");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("El Id del producto es necesario");
        }
    }
}
