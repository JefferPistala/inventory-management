using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.Exceptions;

namespace Transactions.Api.Middleware;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int status, string title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Regla de negocio no cumplida"),
            InsufficientStockException => (StatusCodes.Status409Conflict, "Stock insuficiente"),
            ExternalServiceException => (StatusCodes.Status503ServiceUnavailable, "Servicio de productos no disponible"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error procesando {Path}", httpContext.Request.Path);

        // En un 500 no devuelvo el mensaje real para no exponer detalles internos.
        string detail = status == StatusCodes.Status500InternalServerError
            ? "Ocurrió un error inesperado. Intente más tarde."
            : exception.Message;

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }
}
