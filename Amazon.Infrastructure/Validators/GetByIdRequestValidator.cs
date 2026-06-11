using Amazon.Core.CustomEntities;
using FluentValidation;

namespace Amazon.Infrastructure.Validators
{
    public class GetByIdRequestValidator : AbstractValidator<GetByIdRequest>
    {
        public GetByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("El ID es requerido")
                .GreaterThan(0).WithMessage("El ID debe ser mayor a 0")
                .LessThanOrEqualTo(1000000).WithMessage("El ID no puede ser mayor a 1,000,000")
                .Must(BeAValidIdFormat).WithMessage("El ID debe ser un número válido");
        }

        private bool BeAValidIdFormat(int id)
        {
            return id.ToString().Length <= 7;
        }
    }
}
