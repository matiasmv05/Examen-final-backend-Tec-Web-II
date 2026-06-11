using Amazon.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            // Primero verificar el tipo, nunca asumir
            if (context.Exception is BussinesException businessEx)
            {
                var json = new
                {
                    errors = new[]
                    {
                        new
                        {
                            Status = (int)businessEx.StatusCode,
                            Title = businessEx.StatusCode.ToString(),
                            Detail = businessEx.Message
                        }
                    }
                };

                context.Result = new ObjectResult(json)
                {
                    StatusCode = (int)businessEx.StatusCode
                };

                context.ExceptionHandled = true;
                return; // importante: salir después de manejar
            }

            // Cualquier otra excepción no esperada → 500
            // NO expongas el mensaje real en producción
            var serverError = new
            {
                errors = new[]
                {
                    new
                    {
                        Status = 500,
                        Title = "Internal Server Error",
                        Detail = "Ocurrió un error inesperado en el servidor"
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
