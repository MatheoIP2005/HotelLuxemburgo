using FluentValidation;
using HotelLux.Auth.API.Services;
using HotelLux.Auth.Business.Interfaces;
using HotelLux.Auth.Business.Services;
using HotelLux.Auth.Business.Validators;
using HotelLux.Auth.DataAccess.Context;
using HotelLux.Auth.DataAccess.Repositories;
using HotelLux.Auth.DataAccess.Repositories.Interfaces;
using HotelLux.Auth.DataManagement.Interfaces;
using HotelLux.Auth.DataManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Auth.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuditEmitter, AuditRabbitMqEmitter>();

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("AuthDb"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null)));

        services.AddScoped<IUsuarioAppRepository, UsuarioAppRepository>();
        services.AddScoped<IRolRepository, RolRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioDataService, UsuarioDataService>();
        services.AddScoped<IRolDataService, RolDataService>();

        services.AddSingleton<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<IPermisoService, PermisoService>();

        services.AddValidatorsFromAssemblyContaining<UsuarioValidator>();

        return services;
    }
}
