using Amazon.infrastructure.DTOs;
using FluentValidation;

namespace Amazon.Infrastructure.Validators
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del producto es requerido")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
                .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerida")
                .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres")
                .MinimumLength(10).WithMessage("La descripción debe tener al menos 10 caracteres");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
                .LessThan(1000000).WithMessage("El precio no puede ser mayor a 1,000,000");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("La categoría es requerida")
                .MaximumLength(100).WithMessage("La categoría no puede exceder 100 caracteres");

            RuleFor(x => x.ImageUrl)
                .Must(UrlImagenValida).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("La URL de la imagen no es válida")
                .MaximumLength(500).WithMessage("La URL de la imagen es demasiado larga");
            RuleFor(x => x.stock)
                 .NotEmpty().WithMessage("La descripción es requerida")
                 .GreaterThan(0).WithMessage("tiene que pedir al menos un producto");

        }

        private bool UrlImagenValida(string url)
        {
            if (string.IsNullOrEmpty(url)) return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp
                       || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
