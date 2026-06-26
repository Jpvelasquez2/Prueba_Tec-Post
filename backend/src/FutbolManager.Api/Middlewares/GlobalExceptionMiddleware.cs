using System.Net;
using System.Text.Json;
using FutbolManager.Application.Common.Exceptions;
using FutbolManager.Domain.Exceptions;

namespace FutbolManager.Api.Middlewares;

/// <summary>
/// Middleware global de manejo de excepciones. Convierte las excepciones
/// no controladas en respuestas <c>ProblemDetails</c> (RFC 7807) con código
/// HTTP coherente.
/// </summary>
/// <remarks>
/// Tabla de traducción:
/// <list type="bullet">
///   <item><see cref="NotFoundException"/> → 404 Not Found.</item>
///   <item><see cref="ConflictException"/> → 409 Conflict.</item>
///   <item><see cref="BusinessRuleException"/> → 400 Bad Request.</item>
///   <item>Cualquier otra → 500 Internal Server Error.</item>
/// </list>
/// </remarks>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>DI constructor.</summary>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Punto de entrada del middleware.</summary>
    /// <param name="context">Contexto HTTP de la petición actual.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>Traduce la excepción a un ProblemDetails con el status code adecuado.</summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = MapStatusCode(exception);

        // Errores 5xx se loggean como Error; 4xx como Warning para no contaminar la severidad.
        if (statusCode >= 500)
            _logger.LogError(exception, "Error no controlado al procesar {Path}", context.Request.Path);
        else
            _logger.LogWarning(exception, "Operación rechazada en {Path}: {Mensaje}", context.Request.Path, exception.Message);

        // El traceId lo emitimos para correlación cliente↔logs.
        var problem = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail = exception.Message,
            traceId = context.TraceIdentifier,
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
    }

    /// <summary>Mapea cada tipo de excepción al código y título HTTP correspondientes.</summary>
    private static (int StatusCode, string Title) MapStatusCode(Exception exception) => exception switch
    {
        NotFoundException     => ((int)HttpStatusCode.NotFound,    "Recurso no encontrado"),
        ConflictException     => ((int)HttpStatusCode.Conflict,    "Conflicto con el estado actual"),
        BusinessRuleException => ((int)HttpStatusCode.BadRequest,  "Regla de negocio violada"),
        _                     => ((int)HttpStatusCode.InternalServerError, "Error inesperado"),
    };
}
