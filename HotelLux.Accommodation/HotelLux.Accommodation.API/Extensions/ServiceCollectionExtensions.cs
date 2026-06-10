using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Services;
using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Repositories;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Services;
using HotelLux.Accommodation.API.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccommodationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AccommodationDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("AccommodationDb")));

        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<ISucursalImagenRepository, SucursalImagenRepository>();
        services.AddScoped<ITipoHabitacionRepository, TipoHabitacionRepository>();
        services.AddScoped<ITipoHabitacionImagenRepository, TipoHabitacionImagenRepository>();
        services.AddScoped<IHabitacionRepository, HabitacionRepository>();
        services.AddScoped<ITarifaRepository, TarifaRepository>();
        services.AddScoped<ICatalogoServicioRepository, CatalogoServicioRepository>();
        services.AddScoped<ITipoHabitacionCatalogoRepository, TipoHabitacionCatalogoRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ISucursalDataService, SucursalDataService>();
        services.AddScoped<ISucursalImagenDataService, SucursalImagenDataService>();
        services.AddScoped<ITipoHabitacionDataService, TipoHabitacionDataService>();
        services.AddScoped<ITipoHabitacionImagenDataService, TipoHabitacionImagenDataService>();
        services.AddScoped<IHabitacionDataService, HabitacionDataService>();
        services.AddScoped<ITarifaDataService, TarifaDataService>();
        services.AddScoped<ICatalogoServicioDataService, CatalogoServicioDataService>();
        services.AddScoped<ITipoHabitacionCatalogoDataService, TipoHabitacionCatalogoDataService>();

        services.AddScoped<ISucursalService, SucursalService>();
        services.AddScoped<ISucursalImagenService, SucursalImagenService>();
        services.AddScoped<ITipoHabitacionService, TipoHabitacionService>();
        services.AddScoped<ITipoHabitacionImagenService, TipoHabitacionImagenService>();
        services.AddScoped<IHabitacionService, HabitacionService>();
        services.AddScoped<ITarifaService, TarifaService>();
        services.AddScoped<ICatalogoServicioService, CatalogoServicioService>();
        services.AddScoped<ITipoHabitacionCatalogoService, TipoHabitacionCatalogoService>();

        services.AddSingleton<IAuditEmitter, AuditRabbitMqEmitter>();
        services.AddSingleton<IStayPublicClient, StayPublicGrpcClient>();
        services.AddScoped<PublicHabitacionesListing>();

        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        return services;
    }
}
