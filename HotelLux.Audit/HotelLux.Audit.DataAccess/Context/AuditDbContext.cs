using HotelLux.Audit.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Audit.DataAccess.Context;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<EventoAuditoriaEntity> EventosAuditoria => Set<EventoAuditoriaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auditoria");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
    }
}
