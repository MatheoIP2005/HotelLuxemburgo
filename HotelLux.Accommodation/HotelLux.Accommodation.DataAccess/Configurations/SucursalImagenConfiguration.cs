using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class SucursalImagenConfiguration : IEntityTypeConfiguration<SucursalImagenEntity>
{
    public void Configure(EntityTypeBuilder<SucursalImagenEntity> builder)
    {
        builder.ToTable("sucursal_imagen", "alojamiento");
        builder.HasKey(x => x.IdSucursalImagen);
        builder.Property(x => x.IdSucursalImagen).HasColumnName("id_sucursal_imagen").ValueGeneratedOnAdd();
        builder.Property(x => x.SucursalImagenGuid).HasColumnName("sucursal_imagen_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.IdSucursal).HasColumnName("id_sucursal").IsRequired();
        builder.Property(x => x.UrlImagen).HasColumnName("url_imagen").HasMaxLength(500).IsRequired();
        builder.Property(x => x.DescripcionImagen).HasColumnName("descripcion_imagen").HasMaxLength(255);
        builder.Property(x => x.OrdenVisualizacion).HasColumnName("orden_visualizacion").IsRequired().HasDefaultValue(1);
        builder.Property(x => x.EsPrincipal).HasColumnName("es_principal").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.HasOne(x => x.Sucursal).WithMany(s => s.Imagenes).HasForeignKey(x => x.IdSucursal).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.SucursalImagenGuid).IsUnique();
    }
}
