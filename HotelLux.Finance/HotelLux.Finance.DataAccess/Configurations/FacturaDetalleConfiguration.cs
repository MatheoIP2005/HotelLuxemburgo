using HotelLux.Finance.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Finance.DataAccess.Configurations;

public class FacturaDetalleConfiguration : IEntityTypeConfiguration<FacturaDetalleEntity>
{
    public void Configure(EntityTypeBuilder<FacturaDetalleEntity> builder)
    {
        builder.ToTable("factura_detalle", "finanzas");
        builder.HasKey(x => x.IdFacturaDetalle);
        builder.Property(x => x.IdFacturaDetalle).HasColumnName("id_factura_detalle").ValueGeneratedOnAdd();
        builder.Property(x => x.FacturaDetalleGuid).HasColumnName("factura_detalle_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.IdFactura).HasColumnName("id_factura").IsRequired();
        builder.Property(x => x.TipoItem).HasColumnName("tipo_item").HasMaxLength(30).IsRequired();
        builder.Property(x => x.ReferenciaTipo).HasColumnName("referencia_tipo").HasMaxLength(30);
        builder.Property(x => x.ReferenciaGuid).HasColumnName("referencia_guid");
        builder.Property(x => x.DescripcionItem).HasColumnName("descripcion_item").HasMaxLength(250).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired().HasDefaultValue(1);
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.SubtotalLinea).HasColumnName("subtotal_linea").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.ValorIvaLinea).HasColumnName("valor_iva_linea").HasColumnType("numeric(12,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.DescuentoLinea).HasColumnName("descuento_linea").HasColumnType("numeric(12,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.TotalLinea).HasColumnName("total_linea").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.FacturaDetalleGuid).IsUnique();
        builder.HasOne(x => x.Factura).WithMany(f => f.Detalles).HasForeignKey(x => x.IdFactura).OnDelete(DeleteBehavior.Cascade);
    }
}
