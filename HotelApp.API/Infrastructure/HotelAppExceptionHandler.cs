using HotelApp.BL.Constants;
using HotelApp.BL.Exceptions;
using HotelApp.DAL.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Infrastructure
{
    public class HotelAppExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;
        private readonly ILogger<HotelAppExceptionHandler> _logger;

        public HotelAppExceptionHandler(IProblemDetailsService problemDetails, ILogger<HotelAppExceptionHandler> logger)
        {
            _problemDetails = problemDetails;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var problem = exception switch
            {
                EntityValidationException ex => Validation(ex),
                HotelAppException ex => Controlled(ex),
                _ => Unexpected(exception)
            };

            httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }

        private static ValidationProblemDetails Validation(EntityValidationException ex)
        {
            var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [string.Empty] = ex.Errors.ToArray()
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Los datos enviados no son válidos.",
                Detail = ex.Message
            };
            problem.Extensions["code"] = ex.Code;
            return problem;
        }

        private ProblemDetails Controlled(HotelAppException ex)
        {
            var status = StatusFor(ex);

            if (status >= StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Controlled error {Code} returned as {Status}.", ex.Code, status);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = TitleFor(status),
                Detail = ex.Message
            };
            problem.Extensions["code"] = ex.Code;
            return problem;
        }

        private ProblemDetails Unexpected(Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = TitleFor(StatusCodes.Status500InternalServerError),
                Detail = "Ocurrió un error inesperado. Intente de nuevo más tarde."
            };
        }

        private static int StatusFor(HotelAppException ex) => ex switch
        {
            DataAccessException data => data.ErrorType switch
            {
                DataErrorType.RequiredField or DataErrorType.FieldTooLong => StatusCodes.Status400BadRequest,
                DataErrorType.DuplicateKey or DataErrorType.ForeignKey or DataErrorType.Concurrency => StatusCodes.Status409Conflict,
                DataErrorType.Timeout or DataErrorType.Connection => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status500InternalServerError
            },
            BusinessRuleException rule => rule.Code switch
            {
                ErrorCodes.Validation => StatusCodes.Status400BadRequest,
                ErrorCodes.NotFound => StatusCodes.Status404NotFound,
                ErrorCodes.Auth => StatusCodes.Status401Unauthorized,
                ErrorCodes.Forbidden or "RN-014" => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status422UnprocessableEntity
            },
            _ => StatusCodes.Status400BadRequest
        };

        private static string TitleFor(int status) => status switch
        {
            StatusCodes.Status400BadRequest => "La solicitud no es válida.",
            StatusCodes.Status401Unauthorized => "No autenticado.",
            StatusCodes.Status403Forbidden => "No tiene permiso para esta operación.",
            StatusCodes.Status404NotFound => "Recurso no encontrado.",
            StatusCodes.Status409Conflict => "La operación entra en conflicto con los datos existentes.",
            StatusCodes.Status422UnprocessableEntity => "La operación no cumple una regla de negocio.",
            StatusCodes.Status503ServiceUnavailable => "La base de datos no está disponible.",
            _ => "Error interno del servidor."
        };
    }
}
