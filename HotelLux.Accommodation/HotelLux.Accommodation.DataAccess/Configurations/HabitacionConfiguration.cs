using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class HabitacionConfiguration : IEntityTypeConfiguration<HabitacionEntity>
{
    public void Configure(EntityTypeBuilder<HabitacionEntity> builder)
    {
        builder.ToTable("habitacion", "alojamiento");
        builder.HasKey(x => x.IdHabitacion);
        builder.Property(x => x.IdHabitacion).HasColumnName("id_habitacion").ValueGeneratedOnAdd();
        builder.Property(x => x.HabitacionGuid).HasColumnName("habitacion_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.IdSucursal).HasColumnName("id_sucursal").IsRequired();
        builder.Property(x => x.IdTipoHabitacion).HasColumnName("id_tipo_habitacion").IsRequired();
        builder.Property(x => x.NumeroHabitacion).HasColumnName("numero_habitacion").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Piso).HasColumnName("piso");
        builder.Property(x => x.CapacidadHabitacion).HasColumnName("capacidad_habitacion").IsRequired();
        builder.Property(x => x.PrecioBase).HasColumnName("precio_base").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.DescripcionHabitacion).HasColumnName("descripcion_habitacion");
        builder.Property(x => x.EstadoHabitacion).HasColumnName("estado_habitacion").HasMaxLength(3).IsRequired().HasDefaultValue("DIS");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired().HasDefaultValue("accommodation-service");
        builder.HasOne(x => x.Sucursal).WithMany(s => s.Habitaciones).HasForeignKey(x => x.IdSucursal).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TipoHabitacion).WithMany(t => t.Habitaciones).HasForeignKey(x => x.IdTipoHabitacion).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.HabitacionGuid).IsUnique();
        builder.HasIndex(x => new { x.IdSucursal, x.NumeroHabitacion }).IsUnique();
    }
}
