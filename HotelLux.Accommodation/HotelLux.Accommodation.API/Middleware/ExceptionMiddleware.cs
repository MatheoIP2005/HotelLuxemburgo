using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.Exceptions;
using System.Text.Json;

namespace HotelLux.Accommodation.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (NotFoundException ex)
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(
                ApiErrorResponse.Fail(404, ex.Message)));
        }
        catch (ValidationException ex)
        {
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(
                ApiErrorResponse.Fail(400, ex.Message, ex.Errors)));
        }
        catch (ConflictException ex)
        {
            ctx.Response.StatusCode = 409;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(
                ApiErrorResponse.Fail(409, ex.Message)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(
                ApiErrorResponse.Fail(500, "Error interno del servidor.")));
        }
    }
}
