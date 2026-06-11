using Amazon.Infrastructure.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Validators
{
    public class PaymentDtoValidator : AbstractValidator<PaymentDto>
    {
        public PaymentDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("El estado del pago es requerido")
                .Must(BeValidPaymentStatus).WithMessage("Estado de pago no válido. Valores permitidos: Pending, Completed, Failed");

            RuleFor(x => x.CreatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha de creación esta mal configurada");
        }

        private bool BeValidPaymentStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Completed", "Failed" };
            return validStatuses.Contains(status);
        }
    }
}
