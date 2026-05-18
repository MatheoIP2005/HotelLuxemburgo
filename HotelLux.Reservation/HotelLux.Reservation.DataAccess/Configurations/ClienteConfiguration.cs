using HotelLux.Reservation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelLux.Reservation.DataAccess.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<ClienteEntity>
{
    public void Configure(EntityTypeBuilder<ClienteEntity> builder)
    {
        builder.ToTable("cliente", "reservas");
        builder.HasKey(x => x.IdCliente);

        builder.Property(x => x.IdCliente).HasColumnName("id_cliente").ValueGeneratedOnAdd();
        builder.Property(x => x.ClienteGuid).HasColumnName("cliente_guid")
            .HasDefaultValueSql("gen_random_uuid()").IsRequired();
        builder.Property(x => x.TipoIdentificacion).HasColumnName("tipo_identificacion").HasMaxLength(20).IsRequired();
        builder.Property(x => x.NumeroIdentificacion).HasColumnName("numero_identificacion").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(160);
        builder.Property(x => x.RazonSocial).HasColumnName("razon_social").HasMaxLength(200);
        builder.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(250).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(3).IsRequired().HasDefaultValue("ACT");
        builder.Property(x => x.EsEliminado).HasColumnName("es_eliminado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreadoPorUsuario).HasColumnName("creado_por_usuario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.FechaRegistroUtc).HasColumnName("fecha_registro_utc").HasColumnType("TIMESTAMPTZ").IsRequired();
        builder.Property(x => x.ModificadoPorUsuario).HasColumnName("modificado_por_usuario").HasMaxLength(100);
        builder.Property(x => x.FechaModificacionUtc).HasColumnName("fecha_modificacion_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.ModificacionIp).HasColumnName("modificacion_ip").HasMaxLength(45);
        builder.Property(x => x.FechaInhabilitacionUtc).HasColumnName("fecha_inhabilitacion_utc").HasColumnType("TIMESTAMPTZ");
        builder.Property(x => x.MotivoInhabilitacion).HasColumnName("motivo_inhabilitacion").HasMaxLength(250);
        builder.Property(x => x.ServicioOrigen).HasColumnName("servicio_origen").HasMaxLength(50).IsRequired()
            .HasDefaultValue("reservation-service");

        builder.HasIndex(x => x.ClienteGuid).IsUnique();
        builder.HasIndex(x => x.NumeroIdentificacion).IsUnique();
        builder.HasIndex(x => x.Correo).IsUnique();
        builder.HasIndex(x => new { x.TipoIdentificacion, x.NumeroIdentificacion, x.Correo });
    }
}
