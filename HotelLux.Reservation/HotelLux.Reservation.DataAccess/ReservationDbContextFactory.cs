using HotelLux.Reservation.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelLux.Reservation.DataAccess;

/// <summary>
/// Permite <c>dotnet ef migrations</c> sin arrancar la API (JWT / Kestrel).
/// </summary>
public class ReservationDbContextFactory : IDesignTimeDbContextFactory<ReservationDbContext>
{
    public ReservationDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ReservationDb")
            ?? "Host=localhost;Port=5432;Database=HotelLux_Reservation;Username=postgres;Password=BD081205;SearchPath=reservas";

        var options = new DbContextOptionsBuilder<ReservationDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new ReservationDbContext(options);
    }
}
