using HotelLux.Accommodation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Accommodation.DataAccess.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<SucursalEntity>
{
    public void Configure(EntityTypeBuilder<SucursalEntity> builder)
    {
        builder.ToTable("sucursal", "alojamiento");
        builder.HasKey(x => x.IdSucursal);
        builder.Property(x => x.IdSucursal).HasColumnName("id_sucursal").ValueGeneratedOnAdd();
        builder.Property(x => x.SucursalGuid).HasColumnName("sucursal_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.CodigoSucursal).HasColumnName("codigo_sucursal").HasMaxLength(30).IsRequired();
        builder.Property(x => x.NombreSucursal).HasColumnName("nombre_sucursal").HasMaxLength(150).IsRequired();
        builder.Property(x => x.DescripcionSucursal).HasColumnName("descripcion_sucursal");
        builder.Property(x => x.DescripcionCorta).HasColumnName("descripcion_corta").HasMaxLength(250);
        builder.Property(x => x.TipoAlojamiento).HasColumnName("tipo_alojamiento").HasMaxLength(20).IsRequired().HasDefaultValue("hotel");
        builder.Property(x => x.Estrellas).HasColumnName("estrellas");
        builder.Property(x => x.CategoriaViaje).HasColumnName("categoria_viaje").HasMaxLength(30);
        builder.Property(x => x.Pais).HasColumnName("pais").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Provincia).HasColumnName("provincia").HasMaxLength(100);
        builder.Property(x => x.Ciudad).HasColumnName("ciudad").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Ubicacion).HasColumnName("ubicacion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(250).IsRequired();
        builder.Property(x => x.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(20);
        builder.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Latitud).HasColumnName("latitud").HasColumnType("numeric(10,7)");
        builder.Property(x => x.Longitud).HasColumnName("longitud").HasColumnType("numeric(10,7)");
        builder.Property(x => x.HoraCheckin).HasColumnName("hora_checkin").HasMaxLength(5).HasDefaultValue("15:00");
        builder.Property(x => x.HoraCheckout).HasColumnName("hora_checkout").HasMaxLength(5).HasDefaultValue("12:00");
        builder.Property(x => x.CheckinAnticipado).HasColumnName("checkin_anticipado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CheckoutTardio).HasColumnName("checkout_tardio").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.AceptaNinos).HasColumnName("acepta_ninos").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.EdadMinimaHuesped).HasColumnName("edad_minima_huesped");
        builder.Property(x => x.PermiteMascotas).HasColumnName("permite_mascotas").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.SePermiteFumar).HasColumnName("se_permite_fumar").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.EstadoSucursal).HasColumnName("estado_sucursal").HasMaxLength(3).IsRequired().HasDefaultValue("ACT");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc");
        builder.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").IsRequired();
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired().HasDefaultValue("accommodation-service");
        builder.HasIndex(x => x.SucursalGuid).IsUnique();
        builder.HasIndex(x => x.CodigoSucursal).IsUnique();
        builder.HasIndex(x => x.NombreSucursal).IsUnique();
    }
}
