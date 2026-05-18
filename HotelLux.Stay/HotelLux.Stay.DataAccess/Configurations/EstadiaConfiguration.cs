using HotelLux.Stay.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Stay.DataAccess.Configurations;

public class EstadiaConfiguration : IEntityTypeConfiguration<EstadiaEntity>
{
    public void Configure(EntityTypeBuilder<EstadiaEntity> builder)
    {
        builder.ToTable("estadia", "hospedaje");
        builder.HasKey(x => x.IdEstadia);

        builder.Property(x => x.IdEstadia).HasColumnName("id_estadia").ValueGeneratedOnAdd();
        builder.Property(x => x.EstadiaGuid).HasColumnName("estadia_guid")
            .HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.ReservaGuid).HasColumnName("reserva_guid").IsRequired();
        builder.Property(x => x.ReservaHabitacionGuid).HasColumnName("reserva_habitacion_guid").IsRequired();
        builder.Property(x => x.ClienteGuid).HasColumnName("cliente_guid").IsRequired();
        builder.Property(x => x.SucursalGuid).HasColumnName("sucursal_guid").IsRequired();
        builder.Property(x => x.HabitacionGuid).HasColumnName("habitacion_guid").IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado_estadia").HasMaxLength(3).IsRequired();
        builder.Property(x => x.FechaCheckinUtc).HasColumnName("checkin_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.FechaCheckoutUtc).HasColumnName("checkout_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.ObservacionesCheckin)
            .HasColumnName("observaciones_checkin").HasMaxLength(500);
        builder.Property(x => x.ObservacionesCheckout)
            .HasColumnName("observaciones_checkout").HasMaxLength(500);
        builder.Property(x => x.RequiereMantenimiento)
            .HasColumnName("requiere_mantenimiento").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired()
            .HasDefaultValue("stay-service");

        builder.HasIndex(x => x.EstadiaGuid).IsUnique();
        builder.HasIndex(x => new { x.ReservaGuid, x.Estado });
    }
}
