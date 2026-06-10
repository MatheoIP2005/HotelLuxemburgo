using HotelLux.Gateway.GraphQL;
using HotelLux.Gateway.Swagger;
using HotelLux.Shared.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureHotelLuxLogging();

builder.WebHost.ConfigureHotelLuxKestrel(builder.Environment, defaultHttpPort: 5000, defaultGrpcPort: 5000);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

builder.Services.AddGatewaySwagger();
builder.Services.AddGatewayGraphQl(builder.Configuration);

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Redirect("/swagger"));
var gatewayHealth = () => Results.Json(new
{
    status = "ok",
    service = "HotelLux.Gateway",
    documentation = "/swagger",
    openapi = "/swagger/v1/swagger.json",
    graphql = "/graphql",
    utc = DateTime.UtcNow
});

app.MapGet("/health", gatewayHealth);
app.MapGet("/health/live", gatewayHealth);
app.MapGet("/health/ready", gatewayHealth);

app.UseGatewaySwaggerUi();
app.MapGraphQL("/graphql");
app.MapReverseProxy();

app.Run();
