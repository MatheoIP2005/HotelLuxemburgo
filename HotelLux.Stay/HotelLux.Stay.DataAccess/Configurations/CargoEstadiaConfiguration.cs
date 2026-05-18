using HotelLux.Stay.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Stay.DataAccess.Configurations;

public class CargoEstadiaConfiguration : IEntityTypeConfiguration<CargoEstadiaEntity>
{
    public void Configure(EntityTypeBuilder<CargoEstadiaEntity> builder)
    {
        builder.ToTable("cargo_estadia", "hospedaje");
        builder.HasKey(x => x.IdCargoEstadia);

        builder.Property(x => x.IdCargoEstadia).HasColumnName("id_cargo_estadia").ValueGeneratedOnAdd();
        builder.Property(x => x.CargoGuid).HasColumnName("cargo_guid")
            .HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.IdEstadia).HasColumnName("id_estadia").IsRequired();
        builder.Property(x => x.CatalogoGuid).HasColumnName("catalogo_guid");
        builder.Property(x => x.DescripcionCargo).HasColumnName("descripcion_cargo").HasMaxLength(250).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired().HasDefaultValue(1);
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.ValorIva).HasColumnName("valor_iva").HasColumnType("numeric(12,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.TotalCargo).HasColumnName("total_cargo").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.FechaConsumoUtc).HasColumnName("fecha_consumo_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.EstadoCargo).HasColumnName("estado_cargo").HasMaxLength(3).IsRequired().HasDefaultValue("PEN");
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired()
            .HasDefaultValue("stay-service");

        builder.HasIndex(x => x.CargoGuid).IsUnique();
        builder.HasIndex(x => new { x.IdEstadia, x.EstadoCargo, x.FechaConsumoUtc });

        builder.HasOne(x => x.Estadia)
            .WithMany(e => e.Cargos)
            .HasForeignKey(x => x.IdEstadia);
    }
}
