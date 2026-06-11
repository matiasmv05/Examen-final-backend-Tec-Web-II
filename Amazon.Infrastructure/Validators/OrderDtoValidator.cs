using Amazon.infrastructure.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Validators
{
    public class OrderDtoValidator : AbstractValidator<OrderDto>
    {
        public OrderDtoValidator()
        {
            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("El total debe ser mayor o igual a 0");

            RuleForEach(x => x.OrderItems).SetValidator(new OrderItemDtoValidator());

                
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Cart", "Pending", "Paid", "Cancelled" };
            return validStatuses.Contains(status);
        }
    }

    
}
