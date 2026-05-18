using HotelLux.Auth.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Auth.DataAccess.Configurations;

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRolEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioRolEntity> builder)
    {
        builder.ToTable("usuarios_roles");

        builder.HasKey(e => e.IdUsuarioRol);

        builder.Property(e => e.IdUsuarioRol).HasColumnName("id_usuario_rol");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.IdRol).HasColumnName("id_rol");
        builder.Property(e => e.EstadoUsuarioRol).HasColumnName("estado_usuario_rol").HasMaxLength(3);
        builder.Property(e => e.EsEliminado).HasColumnName("es_eliminado");
        builder.Property(e => e.Activo).HasColumnName("activo");
        builder.Property(e => e.FechaRegistroUtc).HasColumnName("fecha_registro_utc");
        builder.Property(e => e.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(e => e.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(e => e.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);

        builder.HasIndex(e => new { e.IdUsuario, e.IdRol }).IsUnique();
    }
}
