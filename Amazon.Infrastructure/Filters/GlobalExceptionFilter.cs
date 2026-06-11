using Amazon.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Amazon.Infrastructure.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            // Log completo del error real
            _logger.LogError(context.Exception, 
                "EXCEPCION CAPTURADA: {Message} | Inner: {Inner} | Stack: {Stack}",
                context.Exception.Message,
                context.Exception.InnerException?.Message ?? "ninguna",
                context.Exception.StackTrace);

            if (context.Exception is BussinesException businessEx)
            {
                var json = new
                {
                    errors = new[]
                    {
                        new
                        {
                            status = (int)businessEx.StatusCode,
                            title  = businessEx.StatusCode.ToString(),
                            detail = businessEx.Message
                        }
                    }
                };

                context.Result = new ObjectResult(json)
                {
                    StatusCode = (int)businessEx.StatusCode
                };

                context.ExceptionHandled = true;
                return;
            }

            var serverError = new
            {
                errors = new[]
                {
                    new
                    {
                        status = 500,
                        title  = "Internal Server Error",
                        detail = "Ocurrió un error inesperado en el servidor"
                    }
                }
            };

            context.Result = new ObjectResult(serverError)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}