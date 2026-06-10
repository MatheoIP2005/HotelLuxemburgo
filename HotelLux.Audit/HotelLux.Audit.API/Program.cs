using System.Text;
using Asp.Versioning;
using Grpc.AspNetCore.Web;
using HotelLux.Audit.API.Consumers;
using HotelLux.Audit.API.Extensions;
using HotelLux.Audit.API.GrpcServices;
using HotelLux.Shared.Hosting;
using HotelLux.Shared.Messaging;
using MassTransit;
using HotelLux.Audit.DataAccess.Context;
using HotelLux.Audit.DataAccess.Repositories;
using HotelLux.Audit.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<AuditDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("AuditDb")));
builder.Services.AddScoped<IEventoAuditoriaRepository, EventoAuditoriaRepository>();

var rabbitMqSettings = builder.Configuration.GetRabbitMqSettings();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuditEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMqHost(rabbitMqSettings);
        cfg.ReceiveEndpoint(rabbitMqSettings.AuditQueue, e =>
        {
            e.ConfigureConsumer<AuditEventConsumer>(context);
        });
    });
});

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedSwagger(
    "HotelLux Audit API",
    "Auditoría (endpoints internos).");
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.WebHost.ConfigureHotelLuxKestrel(builder.Environment, defaultHttpPort: 5008, defaultGrpcPort: 5108);

var app = builder.Build();
app.UseMiddleware<HotelLux.Audit.API.Middleware.ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseGrpcWeb();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<AuditGrpcService>().EnableGrpcWeb();
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
