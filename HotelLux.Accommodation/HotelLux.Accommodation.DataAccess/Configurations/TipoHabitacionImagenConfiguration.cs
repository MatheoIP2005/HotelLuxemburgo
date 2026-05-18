using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class TipoHabitacionImagenConfiguration : IEntityTypeConfiguration<TipoHabitacionImagenEntity>
{
    public void Configure(EntityTypeBuilder<TipoHabitacionImagenEntity> builder)
    {
        builder.ToTable("tipo_habitacion_imagen", "alojamiento");
        builder.HasKey(x => x.IdTipoHabitacionImagen);
        builder.Property(x => x.IdTipoHabitacionImagen).HasColumnName("id_tipo_habitacion_imagen").ValueGeneratedOnAdd();
        builder.Property(x => x.IdTipoHabitacion).HasColumnName("id_tipo_habitacion").IsRequired();
        builder.Property(x => x.UrlImagen).HasColumnName("url_imagen").HasMaxLength(500).IsRequired();
        builder.Property(x => x.DescripcionImagen).HasColumnName("descripcion_imagen").HasMaxLength(255);
        builder.Property(x => x.OrdenVisualizacion).HasColumnName("orden_visualizacion").HasColumnType("smallint").IsRequired().HasDefaultValue(1);
        builder.Property(x => x.EsPrincipal).HasColumnName("es_principal").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.HasOne(x => x.TipoHabitacion).WithMany(t => t.Imagenes).HasForeignKey(x => x.IdTipoHabitacion).OnDelete(DeleteBehavior.Restrict);
    }
}
