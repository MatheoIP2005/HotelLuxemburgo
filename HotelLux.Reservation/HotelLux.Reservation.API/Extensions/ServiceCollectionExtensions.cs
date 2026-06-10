using HotelLux.Reservation.API.Clients;
using HotelLux.Reservation.API.Services;
using HotelLux.Reservation.Business.Interfaces;
using HotelLux.Reservation.Business.Services;
using HotelLux.Reservation.DataAccess.Context;
using HotelLux.Reservation.DataAccess.Repositories;
using HotelLux.Reservation.DataAccess.Repositories.Interfaces;
using HotelLux.Reservation.DataManagement;
using HotelLux.Reservation.DataManagement.Interfaces;
using HotelLux.Reservation.DataManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Reservation.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReservationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReservationDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("ReservationDb")));

        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IReservaHabitacionRepository, ReservaHabitacionRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IReservaDataService, ReservaDataService>();
        services.AddScoped<IReservaHabitacionDataService, ReservaHabitacionDataService>();
        services.AddScoped<IClienteDataService, ClienteDataService>();

        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IClienteService, ClienteService>();

        services.AddSingleton<IAccommodationClient, AccommodationGrpcClient>();
        services.AddSingleton<IFinanceClient, FinanceGrpcClient>();
        services.AddSingleton<IStayClient, StayGrpcClient>();

        services.AddSingleton<IAuditEmitter, AuditRabbitMqEmitter>();

        services.AddHttpContextAccessor();

        return services;
    }
}
