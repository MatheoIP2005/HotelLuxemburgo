using HotelLux.Reservation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Reservation.DataAccess.Context;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options) : base(options) { }

    public DbSet<ReservaEntity> Reservas => Set<ReservaEntity>();
    public DbSet<ReservaHabitacionEntity> ReservasHabitaciones => Set<ReservaHabitacionEntity>();
    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reservas");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationDbContext).Assembly);
    }
}
