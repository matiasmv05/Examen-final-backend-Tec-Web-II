using Amazon.Infrastructure.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IValidationService _validationService;
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IValidationService validationService, IServiceProvider serviceProvider)
        {
            _validationService = validationService;
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var argumentType = argument.GetType();

                // Verificar si existe un validador para este tipo
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                var validator = _serviceProvider.GetService(validatorType);

                if (validator == null) continue; // No hay validador, saltar

                try
                {
                    // Llamar al servicio de validación con el tipo correcto
                    var method = typeof(IValidationService).GetMethod("ValidateAsync");
                    var genericMethod = method.MakeGenericMethod(argumentType);
                    var validationTask = (Task<ValidationResult>)genericMethod.Invoke(_validationService, new[] { argument });

                    var validationResult = await validationTask;

                    if (!validationResult.IsValid)
                    {
                        context.Result = new BadRequestObjectResult(new { Errors = validationResult.Errors });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't stop execution
                    Console.WriteLine($"Error durante la validación: {ex.Message}");
                }
            }

            await next();
        }
    }
}
