using HotelLux.Finance.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Finance.DataAccess.Context;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

    public DbSet<FacturaEntity> Facturas => Set<FacturaEntity>();
    public DbSet<FacturaDetalleEntity> FacturaDetalles => Set<FacturaDetalleEntity>();
    public DbSet<PagoEntity> Pagos => Set<PagoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finanzas");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
    }
}
