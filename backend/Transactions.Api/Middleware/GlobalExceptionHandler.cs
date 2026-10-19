using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.Exceptions;

namespace Transactions.Api.Middleware;

// Convierto cualquier excepción en una respuesta ProblemDetails con el código
// HTTP correcto. Así no repito try/catch en los controllers y el frontend
// siempre recibe los errores con la misma forma.
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
            // 503: el problema no es la petición, es que Productos no está disponible.
            ExternalServiceException => (StatusCodes.Status503ServiceUnavailable, "Servicio de productos no disponible"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error procesando {Path}", httpContext.Request.Path);

        // En un 500 no expongo el mensaje interno; en los demás, el mensaje
        // ya está pensado para el usuario.
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
