using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class TipoHabitacionConfiguration : IEntityTypeConfiguration<TipoHabitacionEntity>
{
    public void Configure(EntityTypeBuilder<TipoHabitacionEntity> builder)
    {
        builder.ToTable("tipo_habitacion", "alojamiento");
        builder.HasKey(x => x.IdTipoHabitacion);
        builder.Property(x => x.IdTipoHabitacion).HasColumnName("id_tipo_habitacion").ValueGeneratedOnAdd();
        builder.Property(x => x.TipoHabitacionGuid).HasColumnName("tipo_habitacion_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.CodigoTipoHabitacion).HasColumnName("codigo_tipo_habitacion").HasMaxLength(30).IsRequired();
        builder.Property(x => x.NombreTipoHabitacion).HasColumnName("nombre_tipo_habitacion").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion");
        builder.Property(x => x.CapacidadAdultos).HasColumnName("capacidad_adultos").IsRequired();
        builder.Property(x => x.CapacidadNinos).HasColumnName("capacidad_ninos").IsRequired().HasDefaultValue(0);
        builder.Property(x => x.CapacidadTotal).HasColumnName("capacidad_total").IsRequired();
        builder.Property(x => x.TipoCama).HasColumnName("tipo_cama").HasMaxLength(50);
        builder.Property(x => x.AreaM2).HasColumnName("area_m2").HasColumnType("numeric(10,2)");
        builder.Property(x => x.PermiteEventos).HasColumnName("permite_eventos").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.PermiteReservaPublica).HasColumnName("permite_reserva_publica").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.EstadoTipoHabitacion).HasColumnName("estado_tipo_habitacion").HasMaxLength(3).IsRequired().HasDefaultValue("ACT");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired().HasDefaultValue("accommodation-service");
        builder.HasIndex(x => x.TipoHabitacionGuid).IsUnique();
        builder.HasIndex(x => x.CodigoTipoHabitacion).IsUnique();
        builder.HasIndex(x => x.NombreTipoHabitacion).IsUnique();
    }
}
