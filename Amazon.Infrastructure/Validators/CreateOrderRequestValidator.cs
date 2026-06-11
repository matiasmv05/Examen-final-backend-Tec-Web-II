using Amazon.Infrastructure.DTOs;
using FluentValidation;

namespace Amazon.Infrastructure.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("La orden debe tener al menos un item");

            RuleForEach(x => x.OrderItems).SetValidator(new OrderItemRequestValidator());
        }
    }
}
