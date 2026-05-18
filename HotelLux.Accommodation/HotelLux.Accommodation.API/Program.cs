using System.Text;
using Asp.Versioning;
using HotelLux.Accommodation.API.Extensions;
using HotelLux.Accommodation.API.GrpcServices;
using HotelLux.Accommodation.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddGrpc();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedSwagger(
    "HotelLux Accommodation API",
    "Alojamiento: endpoints públicos (accommodations) e internos (catálogo, tarifas, habitaciones).");

builder.Services.AddAccommodationServices(builder.Configuration);

builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var httpPort = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var p) ? p : 5002;
builder.WebHost.ConfigureKestrel(opts =>
{
    opts.ListenAnyIP(httpPort, lo =>
        lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// OpenAPI siempre activo: alimenta el Swagger global del Gateway (/gateway-docs/*).
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var uploadsPhysical = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
Directory.CreateDirectory(uploadsPhysical);
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPhysical),
    RequestPath = "/files"
});

app.MapControllers();
app.MapGrpcService<AccommodationGrpcService>();

app.Run();
