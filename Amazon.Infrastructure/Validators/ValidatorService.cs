using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Amazon.Infrastructure.Validators
{
    public interface IValidationService
    {
        Task<ValidationResult> ValidateAsync<T>(T model);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ValidationResult> ValidateAsync<T>(T model)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();

            if (validator == null)
            {
                throw new InvalidOperationException($"Validación No encontrada para el tipo {typeof(T).Name}");
            }

            var result = await validator.ValidateAsync(model);

            return new ValidationResult
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => e.ErrorMessage).ToList()
            };
        }
    }
}
