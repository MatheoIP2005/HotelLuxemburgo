using HotelLux.Finance.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Finance.DataAccess.Configurations;

public class FacturaConfiguration : IEntityTypeConfiguration<FacturaEntity>
{
    public void Configure(EntityTypeBuilder<FacturaEntity> builder)
    {
        builder.ToTable("factura", "finanzas");
        builder.HasKey(x => x.IdFactura);
        builder.Property(x => x.IdFactura).HasColumnName("id_factura").ValueGeneratedOnAdd();
        builder.Property(x => x.FacturaGuid).HasColumnName("factura_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.ClienteGuid).HasColumnName("cliente_guid").IsRequired();
        builder.Property(x => x.ReservaGuid).HasColumnName("reserva_guid").IsRequired();
        builder.Property(x => x.SucursalGuid).HasColumnName("sucursal_guid").IsRequired();
        builder.Property(x => x.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(40).IsRequired();
        builder.Property(x => x.TipoFactura).HasColumnName("tipo_factura").HasMaxLength(20).IsRequired().HasDefaultValue("RESERVA");
        builder.Property(x => x.FechaEmision).HasColumnName("fecha_emision").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.ValorIva).HasColumnName("valor_iva").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.DescuentoTotal).HasColumnName("descuento_total").HasColumnType("numeric(12,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.SaldoPendiente).HasColumnName("saldo_pendiente").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.Moneda).HasColumnName("moneda").HasMaxLength(10).IsRequired().HasDefaultValue("USD");
        builder.Property(x => x.ObservacionesFactura).HasColumnName("observaciones_factura").HasMaxLength(300);
        builder.Property(x => x.OrigenCanalFactura).HasColumnName("origen_canal_factura").HasMaxLength(50);
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(3).IsRequired().HasDefaultValue("EMI");
        builder.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired().HasDefaultValue("finance-service");
        builder.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.HasIndex(x => x.FacturaGuid).IsUnique();
        builder.HasIndex(x => x.NumeroFactura).IsUnique();
        builder.HasIndex(x => new { x.ReservaGuid, x.Estado, x.FechaEmision });
        builder.HasIndex(x => x.ClienteGuid);
        builder.HasIndex(x => x.SucursalGuid);
    }
}
