using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class TipoHabitacionCatalogoConfiguration : IEntityTypeConfiguration<TipoHabitacionCatalogoEntity>
{
    public void Configure(EntityTypeBuilder<TipoHabitacionCatalogoEntity> builder)
    {
        builder.ToTable("tipo_habitacion_catalogo", "alojamiento");
        builder.HasKey(x => x.IdTipoHabCatalogo);
        builder.Property(x => x.IdTipoHabCatalogo).HasColumnName("id_tipo_hab_catalogo").ValueGeneratedOnAdd();
        builder.Property(x => x.IdTipoHabitacion).HasColumnName("id_tipo_habitacion").IsRequired();
        builder.Property(x => x.IdCatalogo).HasColumnName("id_catalogo").IsRequired();
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.HasOne(x => x.TipoHabitacion).WithMany(t => t.TipoHabitacionCatalogos).HasForeignKey(x => x.IdTipoHabitacion).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.CatalogoServicio).WithMany(c => c.TipoHabitacionCatalogos).HasForeignKey(x => x.IdCatalogo).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.IdTipoHabitacion, x.IdCatalogo }).IsUnique();
    }
}
