using Asp.Versioning.ApiExplorer;
using Grpc.AspNetCore.Web;
using HotelLux.Auth.API.Extensions;
using HotelLux.Auth.API.GrpcServices;
using HotelLux.Shared.Hosting;
using HotelLux.Shared.Messaging;
using HotelLux.Auth.API.Middleware;
using HotelLux.Auth.API.Seeders;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureHotelLuxLogging();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(err =>
                    string.IsNullOrWhiteSpace(err.ErrorMessage)
                        ? $"Campo inválido: {x.Key}"
                        : $"{x.Key}: {err.ErrorMessage}"))
                .ToArray();

            var response = HotelLux.Auth.API.Models.Common.ApiErrorResponse.Fail(
                StatusCodes.Status400BadRequest,
                "La solicitud contiene datos inválidos.",
                details);

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();

builder.Services.AddCustomApiVersioning();
builder.Services.AddCustomCors(builder.Configuration, builder.Environment);
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomSwagger();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddHotelLuxRabbitMqPublisher(builder.Configuration);
builder.Services.AddHostedService<AuthStartupSeeder>();
builder.Services.AddAuthorization();

builder.WebHost.ConfigureHotelLuxKestrel(builder.Environment, defaultHttpPort: 5001, defaultGrpcPort: 5101);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            $"HotelLux Auth API {description.GroupName}");
    }

    options.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseGrpcWeb();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = registration => !registration.Name.Contains("masstransit", StringComparison.OrdinalIgnoreCase)
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => !registration.Name.Contains("masstransit", StringComparison.OrdinalIgnoreCase)
});
app.MapHealthChecks("/health/ready");
app.MapGrpcService<AuthGrpcService>().EnableGrpcWeb();
app.MapControllers();

app.Run();
