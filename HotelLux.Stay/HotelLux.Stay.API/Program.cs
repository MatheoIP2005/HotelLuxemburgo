using System.Text;
using Asp.Versioning;
using Grpc.AspNetCore.Web;
using HotelLux.Stay.API.Extensions;
using HotelLux.Shared.Hosting;
using HotelLux.Shared.Messaging;
using HotelLux.Stay.API.GrpcServices;
using HotelLux.Stay.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureHotelLuxLogging();

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
}).AddApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'V";
    opt.SubstituteApiVersionInUrl = true;
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurada.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedSwagger(
    "HotelLux Stay API",
    "Estadías, cargos y valoraciones (endpoints internos).");
builder.Services.AddStayServices(builder.Configuration);
builder.Services.AddHotelLuxRabbitMqPublisher(builder.Configuration);
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.WebHost.ConfigureHotelLuxKestrel(builder.Environment, defaultHttpPort: 5004, defaultGrpcPort: 5104);

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseGrpcWeb();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<StayGrpcService>().EnableGrpcWeb();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = registration => !registration.Name.Contains("masstransit", StringComparison.OrdinalIgnoreCase)
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => !registration.Name.Contains("masstransit", StringComparison.OrdinalIgnoreCase)
});
app.MapHealthChecks("/health/ready");
app.Run();
