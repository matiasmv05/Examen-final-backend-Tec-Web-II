using Amazon.Core.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace Amazon.Infrastructure.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
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

            // En producción expone el mensaje real temporalmente para debug
            var detail = _env.IsDevelopment()
                ? $"{context.Exception.Message} | {context.Exception.InnerException?.Message} | {context.Exception.StackTrace}"
                : $"{context.Exception.Message} | {context.Exception.InnerException?.Message}"; // ← temporal

            var serverError = new
            {
                errors = new[]
                {
                    new
                    {
                        status = 500,
                        title  = "Internal Server Error",
                        detail
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