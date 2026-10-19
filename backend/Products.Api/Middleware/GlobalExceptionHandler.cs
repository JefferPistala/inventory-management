using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Api.Exceptions;

namespace Products.Api.Middleware;

// Manejo GLOBAL de errores: convierte cada excepción en una respuesta HTTP
// con formato ProblemDetails (estándar RFC 9457). Ventajas:
//  - Los controllers quedan limpios, sin try/catch repetidos.
//  - El frontend siempre recibe errores con la misma forma
//    ({ status, title, detail }) y puede mostrar "detail" al usuario.
public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Traducción excepción → (código HTTP, título).
        (int status, string title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Regla de negocio no cumplida"),
            InsufficientStockException => (StatusCodes.Status409Conflict, "Stock insuficiente"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflicto de concurrencia"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        // Solo los errores inesperados se registran como Error.
        // Los de negocio son parte normal del flujo.
        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado procesando {Path}", httpContext.Request.Path);

        // En un 500 NO se devuelve el mensaje interno (podría filtrar detalles
        // de la base o del código); se da un mensaje genérico.
        string detail = exception switch
        {
            DbUpdateConcurrencyException => "El registro fue modificado por otro usuario. Recargue e intente de nuevo.",
            _ when status == StatusCodes.Status500InternalServerError => "Ocurrió un error inesperado. Intente más tarde.",
            _ => exception.Message
        };

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
