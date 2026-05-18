using HotelLux.Stay.API.Clients;
using HotelLux.Stay.API.Services;
using HotelLux.Stay.Business.Interfaces;
using HotelLux.Stay.Business.Services;
using HotelLux.Stay.DataAccess.Context;
using HotelLux.Stay.DataAccess.Repositories;
using HotelLux.Stay.DataAccess.Repositories.Interfaces;
using HotelLux.Stay.DataManagement;
using HotelLux.Stay.DataManagement.Interfaces;
using HotelLux.Stay.DataManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Stay.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StayDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("StayDb")));

        services.AddScoped<IEstadiaRepository, EstadiaRepository>();
        services.AddScoped<IValoracionRepository, ValoracionRepository>();
        services.AddScoped<ICargoEstadiaRepository, CargoEstadiaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEstadiaDataService, EstadiaDataService>();
        services.AddScoped<IValoracionDataService, ValoracionDataService>();
        services.AddScoped<ICargoEstadiaDataService, CargoEstadiaDataService>();
        services.AddScoped<IEstadiaService, EstadiaService>();
        services.AddScoped<ICargoEstadiaService, CargoEstadiaService>();
        services.AddSingleton<IReservationStayClient, ReservationStayGrpcClient>();
        services.AddSingleton<IAccommodationStayClient, AccommodationStayGrpcClient>();
        services.AddSingleton<IFinanceStayClient, FinanceStayGrpcClient>();
        services.AddScoped<IValoracionService, ValoracionService>();
        services.AddSingleton<IAuditEmitter, AuditGrpcEmitter>();
        services.AddHttpContextAccessor();
        return services;
    }
}
