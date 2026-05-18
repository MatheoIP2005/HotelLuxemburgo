using HotelLux.Auth.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Auth.DataAccess.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<RolEntity>
{
    public void Configure(EntityTypeBuilder<RolEntity> builder)
    {
        builder.ToTable("rol");

        builder.HasKey(e => e.IdRol);

        builder.Property(e => e.IdRol).HasColumnName("id_rol");
        builder.Property(e => e.RolGuid).HasColumnName("rol_guid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();
        builder.Property(e => e.NombreRol).HasColumnName("nombre_rol").HasMaxLength(50);
        builder.Property(e => e.DescripcionRol).HasColumnName("descripcion_rol").HasMaxLength(250);
        builder.Property(e => e.EstadoRol).HasColumnName("estado_rol").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.Activo).HasColumnName("activo");
        builder.Property(e => e.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(e => e.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);

        builder.HasMany(e => e.UsuarioRoles)
            .WithOne(e => e.Rol)
            .HasForeignKey(e => e.IdRol)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
