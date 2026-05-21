using System.Text;
using Asp.Versioning;
using HotelLux.Audit.API.Extensions;
using HotelLux.Audit.API.GrpcServices;
using HotelLux.Audit.DataAccess.Context;
using HotelLux.Audit.DataAccess.Repositories;
using HotelLux.Audit.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedSwagger(
    "HotelLux Audit API",
    "Auditoría (endpoints internos).");
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Kestrel — REST en httpPort (Http1AndHttp2) + listener dedicado gRPC h2c en grpcPort (Http2).
// El AuditService es consumer fire-and-forget vía gRPC, así que el puerto gRPC es el camino real.
var httpPort = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var p) ? p : 5008;
var grpcPort = int.TryParse(Environment.GetEnvironmentVariable("GRPC_PORT"), out var gp) ? gp : 5108;
builder.WebHost.ConfigureKestrel(opts =>
{
    opts.ListenAnyIP(httpPort, lo =>
        lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
    if (grpcPort != httpPort)
    {
        opts.ListenAnyIP(grpcPort, lo =>
            lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
    }
});

var app = builder.Build();
app.UseMiddleware<HotelLux.Audit.API.Middleware.ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<AuditGrpcService>();
app.Run();
