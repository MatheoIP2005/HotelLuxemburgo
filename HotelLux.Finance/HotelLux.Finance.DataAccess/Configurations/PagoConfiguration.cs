using HotelLux.Finance.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Finance.DataAccess.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<PagoEntity>
{
    public void Configure(EntityTypeBuilder<PagoEntity> builder)
    {
        builder.ToTable("pago", "finanzas");
        builder.HasKey(x => x.IdPago);
        builder.Property(x => x.IdPago).HasColumnName("id_pago").ValueGeneratedOnAdd();
        builder.Property(x => x.PagoGuid).HasColumnName("pago_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.IdFactura).HasColumnName("id_factura").IsRequired();
        builder.Property(x => x.ReservaGuid).HasColumnName("reserva_guid").IsRequired();
        builder.Property(x => x.Monto).HasColumnName("monto").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.MetodoPago).HasColumnName("metodo_pago").HasMaxLength(40).IsRequired();
        builder.Property(x => x.EsPagoElectronico).HasColumnName("es_pago_electronico").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ProveedorPasarela).HasColumnName("proveedor_pasarela").HasMaxLength(50);
        builder.Property(x => x.TransaccionExterna).HasColumnName("transaccion_externa").HasMaxLength(150);
        builder.Property(x => x.CodigoAutorizacion).HasColumnName("codigo_autorizacion").HasMaxLength(150);
        builder.Property(x => x.Referencia).HasColumnName("referencia").HasMaxLength(150);
        builder.Property(x => x.EstadoPago).HasColumnName("estado_pago").HasMaxLength(3).IsRequired().HasDefaultValue("PEN");
        builder.Property(x => x.FechaPagoUtc).HasColumnName("fecha_pago_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.Moneda).HasColumnName("moneda").HasMaxLength(10).IsRequired().HasDefaultValue("USD");
        builder.Property(x => x.TipoCambio).HasColumnName("tipo_cambio").HasColumnType("numeric(10,4)").IsRequired().HasDefaultValue(1m);
        builder.Property(x => x.RespuestaPasarela).HasColumnName("respuesta_pasarela");
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired().HasDefaultValue("finance-service");
        builder.HasIndex(x => x.PagoGuid).IsUnique();
        builder.HasIndex(x => new { x.IdFactura, x.EstadoPago, x.FechaPagoUtc });
        builder.HasIndex(x => x.ReservaGuid);
        builder.HasOne(x => x.Factura).WithMany(f => f.Pagos).HasForeignKey(x => x.IdFactura);
    }
}
