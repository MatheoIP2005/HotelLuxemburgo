using HotelLux.Reservation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Reservation.DataAccess.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<ReservaEntity>
{
    public void Configure(EntityTypeBuilder<ReservaEntity> builder)
    {
        builder.ToTable("reserva", "reservas");
        builder.HasKey(x => x.IdReserva);

        builder.Property(x => x.IdReserva)
            .HasColumnName("id_reserva")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ReservaGuid)
            .HasColumnName("reserva_guid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(x => x.CodigoReserva)
            .HasColumnName("codigo_reserva")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.IdCliente)
            .HasColumnName("id_cliente")
            .IsRequired();

        builder.Ignore(x => x.ClienteGuid);

        builder.Property(x => x.SucursalGuid)
            .HasColumnName("sucursal_guid")
            .IsRequired();

        builder.Property(x => x.FechaReservaUtc)
            .HasColumnName("fecha_reserva_utc")
            .HasColumnType("TIMESTAMPTZ")
            .IsRequired();

        builder.Property(x => x.FechaInicio)
            .HasColumnName("fecha_inicio")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.FechaFin)
            .HasColumnName("fecha_fin")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.SubtotalReserva)
            .HasColumnName("subtotal_reserva")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(x => x.ValorIva)
            .HasColumnName("valor_iva")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(x => x.TotalReserva)
            .HasColumnName("total_reserva")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(x => x.DescuentoAplicado)
            .HasColumnName("descuento_aplicado")
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(x => x.SaldoPendiente)
            .HasColumnName("saldo_pendiente")
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(x => x.OrigenCanalReserva)
            .HasColumnName("origen_canal_reserva")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EstadoReserva)
            .HasColumnName("estado_reserva")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("PEN");

        builder.Property(x => x.FechaConfirmacionUtc)
            .HasColumnName("fecha_confirmacion_utc")
            .HasColumnType("TIMESTAMPTZ");

        builder.Property(x => x.FechaCancelacionUtc)
            .HasColumnName("fecha_cancelacion_utc")
            .HasColumnType("TIMESTAMPTZ");

        builder.Property(x => x.MotivoCancelacion)
            .HasColumnName("motivo_cancelacion")
            .HasMaxLength(250);

        builder.Property(x => x.Observaciones)
            .HasColumnName("observaciones")
            .HasMaxLength(500);

        builder.Property(x => x.EsWalkin)
            .HasColumnName("es_walkin")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.EsEliminado)
            .HasColumnName("es_eliminado")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.FechaInhabilitacionUtc)
            .HasColumnName("fecha_inhabilitacion_utc")
            .HasColumnType("TIMESTAMPTZ");

        builder.Property(x => x.MotivoInhabilitacion)
            .HasColumnName("motivo_inhabilitacion")
            .HasMaxLength(250);

        builder.Property(x => x.FechaRegistroUtc)
            .HasColumnName("fecha_registro_utc")
            .HasColumnType("TIMESTAMPTZ")
            .IsRequired();

        builder.Property(x => x.CreadoPorUsuario)
            .HasColumnName("creado_por_usuario")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreadoDesdeIp)
            .HasColumnName("creado_desde_ip")
            .HasMaxLength(45);

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

        builder.HasIndex(x => x.ReservaGuid).IsUnique();
        builder.HasIndex(x => x.CodigoReserva).IsUnique();
        builder.HasIndex(x => new { x.IdCliente, x.EstadoReserva });
        builder.HasIndex(x => new { x.SucursalGuid, x.FechaInicio, x.FechaFin });

        builder.HasOne(x => x.Cliente)
            .WithMany(c => c.Reservas)
            .HasForeignKey(x => x.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
