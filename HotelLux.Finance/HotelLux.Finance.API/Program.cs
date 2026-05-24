using System.Text;
using Asp.Versioning;
using Grpc.AspNetCore.Web;
using HotelLux.Finance.API.Extensions;
using HotelLux.Shared.Hosting;
using HotelLux.Finance.API.Clients;
using HotelLux.Finance.API.GrpcServices;
using HotelLux.Finance.API.Middleware;
using HotelLux.Finance.Business.Interfaces;
using HotelLux.Finance.Business.Services;
using HotelLux.Finance.DataAccess.Context;
using HotelLux.Finance.DataAccess.Repositories;
using HotelLux.Finance.DataAccess.Repositories.Interfaces;
using HotelLux.Finance.DataManagement;
using HotelLux.Finance.DataManagement.Interfaces;
using HotelLux.Finance.DataManagement.Services;
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

builder.Services.AddDbContext<FinanceDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("FinanceDb")));
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFacturaDataService, FacturaDataService>();
builder.Services.AddScoped<IPagoDataService, PagoDataService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddSingleton<IAuditEmitter, AuditGrpcClient>();

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedSwagger(
    "HotelLux Finance API",
    "Facturas y pagos (endpoints internos).");
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.WebHost.ConfigureHotelLuxKestrel(builder.Environment, defaultHttpPort: 5005, defaultGrpcPort: 5105);

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseGrpcWeb();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<FinanceGrpcService>().EnableGrpcWeb();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
