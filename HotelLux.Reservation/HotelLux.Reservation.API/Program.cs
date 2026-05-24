using System.Text;
using Asp.Versioning;
using Grpc.AspNetCore.Web;
using HotelLux.Reservation.API.Extensions;
using HotelLux.Shared.Hosting;
using HotelLux.Reservation.API.GrpcServices;
using HotelLux.Reservation.API.Helpers;
using HotelLux.Reservation.API.Middleware;
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
builder.Services.AddHealthChecks();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedSwagger(
    "HotelLux Reservation API",
    "Reservas y clientes: endpoints públicos (accomodations/reservas) e internos.");

builder.Services.AddReservationServices(builder.Configuration);

builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.WebHost.ConfigureHotelLuxKestrel(builder.Environment, defaultHttpPort: 5003, defaultGrpcPort: 5103);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseGrpcWeb();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ReservationGrpcService>().EnableGrpcWeb();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
