using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Context;

public class AccommodationDbContext : DbContext
{
    public AccommodationDbContext(DbContextOptions<AccommodationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SucursalEntity> Sucursales => Set<SucursalEntity>();
    public DbSet<SucursalImagenEntity> SucursalImagenes => Set<SucursalImagenEntity>();
    public DbSet<TipoHabitacionEntity> TiposHabitacion => Set<TipoHabitacionEntity>();
    public DbSet<TipoHabitacionImagenEntity> TipoHabitacionImagenes => Set<TipoHabitacionImagenEntity>();
    public DbSet<HabitacionEntity> Habitaciones => Set<HabitacionEntity>();
    public DbSet<TarifaEntity> Tarifas => Set<TarifaEntity>();
    public DbSet<CatalogoServicioEntity> CatalogoServicios => Set<CatalogoServicioEntity>();
    public DbSet<TipoHabitacionCatalogoEntity> TipoHabitacionCatalogos => Set<TipoHabitacionCatalogoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("alojamiento");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccommodationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
