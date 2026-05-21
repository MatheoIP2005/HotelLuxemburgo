using System.Text.Json;
using HotelLux.Finance.API.Models.Common;
using HotelLux.Finance.Business.Exceptions;

namespace HotelLux.Finance.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteJson(context, 404, "No encontrado", new[] { ex.Message });
        }
        catch (ValidationException ex)
        {
            await WriteJson(context, 400, "Solicitud inválida", ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await WriteJson(context, 500, "Error interno del servidor", Array.Empty<string>());
        }
    }

    private static Task WriteJson(HttpContext ctx, int status, string error, IEnumerable<string> details)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse
        {
            Status = status,
            Error = error,
            Details = details.ToList()
        }));
    }
}
