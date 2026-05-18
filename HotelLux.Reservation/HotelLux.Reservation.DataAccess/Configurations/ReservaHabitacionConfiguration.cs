using HotelLux.Reservation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Reservation.DataAccess.Configurations;

public class ReservaHabitacionConfiguration : IEntityTypeConfiguration<ReservaHabitacionEntity>
{
    public void Configure(EntityTypeBuilder<ReservaHabitacionEntity> builder)
    {
        builder.ToTable("reserva_habitacion", "reservas");
        builder.HasKey(x => x.IdReservaHabitacion);

        builder.Property(x => x.IdReservaHabitacion)
            .HasColumnName("id_reserva_habitacion")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ReservaHabitacionGuid)
            .HasColumnName("reserva_habitacion_guid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(x => x.IdReserva)
            .HasColumnName("id_reserva");

        builder.Property(x => x.HabitacionGuid)
            .HasColumnName("habitacion_guid")
            .IsRequired();

        builder.Property(x => x.TarifaGuid)
            .HasColumnName("tarifa_guid");

        builder.Property(x => x.FechaInicio)
            .HasColumnName("fecha_inicio")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.FechaFin)
            .HasColumnName("fecha_fin")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.NumAdultos)
            .HasColumnName("num_adultos")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.NumNinos)
            .HasColumnName("num_ninos")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.PrecioNocheAplicado)
            .HasColumnName("precio_noche_aplicado")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(x => x.SubtotalLinea)
            .HasColumnName("subtotal_linea")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(x => x.ValorIvaLinea)
            .HasColumnName("valor_iva_linea")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(x => x.DescuentoLinea)
            .HasColumnName("descuento_linea")
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(x => x.TotalLinea)
            .HasColumnName("total_linea")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(x => x.EstadoDetalle)
            .HasColumnName("estado_detalle")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("PEN");

        builder.Property(x => x.FechaRegistroUtc)
            .HasColumnName("fecha_registro_utc")
            .HasColumnType("TIMESTAMPTZ")
            .IsRequired();

        builder.Property(x => x.CreadoPorUsuario)
            .HasColumnName("creado_por_usuario")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ModificadoPorUsuario)
            .HasColumnName("modificado_por_usuario")
            .HasMaxLength(100);

        builder.Property(x => x.FechaModificacionUtc)
            .HasColumnName("fecha_modificacion_utc")
            .HasColumnType("TIMESTAMPTZ");

        builder.Property(x => x.ModificacionIp)
            .HasColumnName("modificacion_ip")
            .HasMaxLength(45);

        builder.Property(x => x.ServicioOrigen)
            .HasColumnName("servicio_origen")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("reservation-service");

        builder.HasIndex(x => x.ReservaHabitacionGuid).IsUnique();
        builder.HasIndex(x => new { x.IdReserva, x.HabitacionGuid, x.FechaInicio }).IsUnique();

        builder.HasOne(x => x.Reserva)
            .WithMany(x => x.ReservasHabitaciones)
            .HasForeignKey(x => x.IdReserva)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
