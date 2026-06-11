using Amazon.Core.CustomEntities;
using Amazon.infrastructure.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Validators
{
    public class CrearOrdenRequestValidation : AbstractValidator<CrearOrdenRequest>
    {
        public CrearOrdenRequestValidation()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El Id del usuario es necesario");
            RuleForEach(x => x.OrderItems).SetValidator(new OrderItemRequestValidator());

        }
    }
}
