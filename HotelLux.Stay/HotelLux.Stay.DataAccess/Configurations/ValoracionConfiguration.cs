using HotelLux.Stay.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Stay.DataAccess.Configurations;

public class ValoracionConfiguration : IEntityTypeConfiguration<ValoracionEntity>
{
    public void Configure(EntityTypeBuilder<ValoracionEntity> builder)
    {
        builder.ToTable("valoracion", "hospedaje");
        builder.HasKey(x => x.IdValoracion);

        builder.Property(x => x.IdValoracion).HasColumnName("id_valoracion").ValueGeneratedOnAdd();
        builder.Property(x => x.ValoracionGuid).HasColumnName("valoracion_guid")
            .HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.EstadiaGuid).HasColumnName("estadia_guid").IsRequired();
        builder.Property(x => x.SucursalGuid).HasColumnName("sucursal_guid").IsRequired();
        builder.Property(x => x.ClienteGuid).HasColumnName("cliente_guid").IsRequired();
        builder.Property(x => x.PuntuacionGeneral).HasColumnName("puntuacion_general").HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(x => x.PuntuacionLimpieza).HasColumnName("puntuacion_limpieza").HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(x => x.PuntuacionConfort).HasColumnName("puntuacion_confort").HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(x => x.PuntuacionUbicacion).HasColumnName("puntuacion_ubicacion").HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(x => x.PuntuacionInstalaciones).HasColumnName("puntuacion_instalaciones").HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(x => x.PuntuacionPersonal).HasColumnName("puntuacion_personal").HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(x => x.PuntuacionCalidadPrecio).HasColumnName("puntuacion_calidad_precio").HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(x => x.ComentarioPositivo).HasColumnName("comentario_positivo").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ComentarioNegativo).HasColumnName("comentario_negativo").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.TipoViaje).HasColumnName("tipo_viaje").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaPublicacionUtc).HasColumnName("fecha_publicacion_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.RespuestaHotel).HasColumnName("respuesta_hotel").HasMaxLength(2000);
        builder.Property(x => x.NombreVisibleCliente)
            .HasColumnName("nombre_visible_cliente")
            .HasMaxLength(150);
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.ValoracionGuid).IsUnique();
        builder.HasIndex(x => x.SucursalGuid);
        builder.HasIndex(x => x.ClienteGuid);
        builder.HasIndex(x => x.EstadiaGuid);
    }
}
