using HotelLux.Audit.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Audit.DataAccess.Configurations;

public class EventoAuditoriaConfiguration : IEntityTypeConfiguration<EventoAuditoriaEntity>
{
    public void Configure(EntityTypeBuilder<EventoAuditoriaEntity> builder)
    {
        builder.ToTable("evento_auditoria", "auditoria");
        builder.HasKey(x => x.IdAuditoria);

        builder.Property(x => x.IdAuditoria).HasColumnName("id_auditoria").ValueGeneratedOnAdd();
        builder.Property(x => x.AuditoriaGuid).HasColumnName("auditoria_guid")
            .HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.TablaAfectada).HasColumnName("tabla_afectada").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Operacion).HasColumnName("operacion").HasMaxLength(10).IsRequired();
        builder.Property(x => x.EntidadGuid).HasColumnName("entidad_guid");
        builder.Property(x => x.IdRegistroAfectado).HasColumnName("id_registro_afectado").HasMaxLength(100);
        builder.Property(x => x.DatosAnteriores).HasColumnName("datos_anteriores").HasColumnType("jsonb");
        builder.Property(x => x.DatosNuevos).HasColumnName("datos_nuevos").HasColumnType("jsonb");
        builder.Property(x => x.UsuarioEjecutor).HasColumnName("usuario_ejecutor").HasMaxLength(100).IsRequired();
        builder.Property(x => x.UsuarioGuid).HasColumnName("usuario_guid");
        builder.Property(x => x.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(80).IsRequired();
        builder.Property(x => x.FechaEventoUtc).HasColumnName("fecha_evento_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => new { x.TablaAfectada, x.FechaEventoUtc });
        builder.HasIndex(x => new { x.UsuarioEjecutor, x.FechaEventoUtc });
        builder.HasIndex(x => new { x.ServicioOrigen, x.FechaEventoUtc });
        builder.HasIndex(x => x.EntidadGuid);
        builder.HasIndex(x => x.FechaEventoUtc);
    }
}
