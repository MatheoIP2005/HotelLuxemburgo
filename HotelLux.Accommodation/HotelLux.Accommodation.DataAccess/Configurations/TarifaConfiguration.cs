using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class TarifaConfiguration : IEntityTypeConfiguration<TarifaEntity>
{
    public void Configure(EntityTypeBuilder<TarifaEntity> builder)
    {
        builder.ToTable("tarifa", "alojamiento");
        builder.HasKey(x => x.IdTarifa);
        builder.Property(x => x.IdTarifa).HasColumnName("id_tarifa").ValueGeneratedOnAdd();
        builder.Property(x => x.TarifaGuid).HasColumnName("tarifa_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.CodigoTarifa).HasColumnName("codigo_tarifa").HasMaxLength(50).IsRequired();
        builder.Property(x => x.IdSucursal).HasColumnName("id_sucursal").IsRequired();
        builder.Property(x => x.IdTipoHabitacion).HasColumnName("id_tipo_habitacion").IsRequired();
        builder.Property(x => x.NombreTarifa).HasColumnName("nombre_tarifa").HasMaxLength(150).IsRequired();
        builder.Property(x => x.CanalTarifa).HasColumnName("canal_tarifa").HasMaxLength(30).IsRequired().HasDefaultValue("TODOS");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").HasColumnType("date").IsRequired();
        builder.Property(x => x.PrecioPorNoche).HasColumnName("precio_por_noche").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.PorcentajeIva).HasColumnName("porcentaje_iva").HasColumnType("numeric(5,2)").IsRequired().HasDefaultValue(15.00m);
        builder.Property(x => x.MinNoches).HasColumnName("min_noches").IsRequired().HasDefaultValue(1);
        builder.Property(x => x.MaxNoches).HasColumnName("max_noches");
        builder.Property(x => x.PermitePortalPublico).HasColumnName("permite_portal_publico").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.Prioridad).HasColumnName("prioridad").IsRequired().HasDefaultValue(1);
        builder.Property(x => x.EstadoTarifa).HasColumnName("estado_tarifa").HasMaxLength(3).IsRequired().HasDefaultValue("ACT");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired().HasDefaultValue("accommodation-service");
        builder.HasOne(x => x.Sucursal).WithMany(s => s.Tarifas).HasForeignKey(x => x.IdSucursal).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TipoHabitacion).WithMany(t => t.Tarifas).HasForeignKey(x => x.IdTipoHabitacion).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TarifaGuid).IsUnique();
        builder.HasIndex(x => x.CodigoTarifa).IsUnique();
    }
}
