using HotelLux.Stay.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Stay.DataAccess.Context;

public class StayDbContext : DbContext
{
    public StayDbContext(DbContextOptions<StayDbContext> options) : base(options) { }

    public DbSet<EstadiaEntity> Estadias => Set<EstadiaEntity>();
    public DbSet<ValoracionEntity> Valoraciones => Set<ValoracionEntity>();
    public DbSet<CargoEstadiaEntity> Cargos => Set<CargoEstadiaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hospedaje");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StayDbContext).Assembly);
    }
}
