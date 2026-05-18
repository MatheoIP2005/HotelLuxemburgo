using HotelLux.Auth.API.Models.Common;
using HotelLux.Auth.Business.Exceptions;
using System.Net;
using System.Text.Json;
using ValidationException = HotelLux.Auth.Business.Exceptions.ValidationException;

namespace HotelLux.Auth.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "ValidationException: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest,
                ApiErrorResponse.Fail(StatusCodes.Status400BadRequest, ex.Message, ex.Errors.ToList()));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "NotFoundException: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotFound,
                ApiErrorResponse.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (UnauthorizedBusinessException ex)
        {
            _logger.LogWarning(ex, "UnauthorizedBusinessException: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized,
                ApiErrorResponse.Fail(StatusCodes.Status401Unauthorized, ex.Message));
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "ConflictException: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Conflict,
                ApiErrorResponse.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "BusinessException: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest,
                ApiErrorResponse.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (NotImplementedException ex)
        {
            _logger.LogInformation(ex, "No implementado: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotImplemented,
                ApiErrorResponse.Fail(StatusCodes.Status501NotImplemented, "Funcionalidad no implementada.",
                    new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError,
                ApiErrorResponse.Fail(StatusCodes.Status500InternalServerError,
                    "Ha ocurrido un error interno en el servidor."));
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, ApiErrorResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
