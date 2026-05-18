using Asp.Versioning.ApiExplorer;
using HotelLux.Auth.API.Extensions;
using HotelLux.Auth.API.GrpcServices;
using HotelLux.Auth.API.Middleware;
using HotelLux.Auth.API.Seeders;
using HotelLux.Auth.DataAccess.Context;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddAuthorization();

var authPort = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var ap) ? ap : 5001;
builder.WebHost.ConfigureKestrel(opts =>
{
    opts.ListenAnyIP(authPort, lo =>
        lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await PasswordSeeder.RegenerarHashesPlaceholderAsync(db, CancellationToken.None);
}

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
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapGrpcService<AuthGrpcService>();

app.Run();
