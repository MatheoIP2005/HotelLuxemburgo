using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class CatalogoServicioConfiguration : IEntityTypeConfiguration<CatalogoServicioEntity>
{
    public void Configure(EntityTypeBuilder<CatalogoServicioEntity> builder)
    {
        builder.ToTable("catalogo_servicios", "alojamiento");
        builder.HasKey(x => x.IdCatalogo);
        builder.Property(x => x.IdCatalogo).HasColumnName("id_catalogo").ValueGeneratedOnAdd();
        builder.Property(x => x.CatalogoGuid).HasColumnName("catalogo_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.IdSucursal).HasColumnName("id_sucursal");
        builder.Property(x => x.CodigoCatalogo).HasColumnName("codigo_catalogo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.NombreCatalogo).HasColumnName("nombre_catalogo").HasMaxLength(150).IsRequired();
        builder.Property(x => x.TipoCatalogo).HasColumnName("tipo_catalogo").HasMaxLength(3).IsRequired();
        builder.Property(x => x.CategoriaCatalogo).HasColumnName("categoria_catalogo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.DescripcionCatalogo).HasColumnName("descripcion_catalogo");
        builder.Property(x => x.PrecioBase).HasColumnName("precio_base").HasColumnType("numeric(12,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.AplicaIva).HasColumnName("aplica_iva").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Disponible24h).HasColumnName("disponible_24h").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.HoraInicio).HasColumnName("hora_inicio").HasColumnType("time");
        builder.Property(x => x.HoraFin).HasColumnName("hora_fin").HasColumnType("time");
        builder.Property(x => x.IconoUrl).HasColumnName("icono_url").HasMaxLength(500);
        builder.Property(x => x.EstadoCatalogo).HasColumnName("estado_catalogo").HasMaxLength(3).IsRequired().HasDefaultValue("ACT");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired().HasDefaultValue("accommodation-service");
        builder.HasOne(x => x.Sucursal).WithMany(s => s.CatalogoServicios).HasForeignKey(x => x.IdSucursal).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.CatalogoGuid).IsUnique();
        builder.HasIndex(x => x.CodigoCatalogo).IsUnique();
    }
}
