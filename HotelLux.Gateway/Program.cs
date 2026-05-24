using HotelLux.Gateway.Swagger;
using HotelLux.Shared.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureHotelLuxKestrel(builder.Environment, defaultHttpPort: 5000, defaultGrpcPort: 5000);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

builder.Services.AddGatewaySwagger();

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Json(new
{
    status = "ok",
    service = "HotelLux.Gateway",
    documentation = "/swagger",
    openapi = "/swagger/v1/swagger.json",
    utc = DateTime.UtcNow
}));

app.UseGatewaySwaggerUi();
app.MapReverseProxy();

app.Run();
