using HotelLux.Auth.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Auth.DataAccess.Context;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<UsuarioAppEntity> UsuarioApps => Set<UsuarioAppEntity>();
    public DbSet<RolEntity> Roles => Set<RolEntity>();
    public DbSet<UsuarioRolEntity> UsuariosRoles => Set<UsuarioRolEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("seguridad");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
