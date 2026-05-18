using HotelLux.Reservation.DataAccess.Entities;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Mappers;

public static class ClienteDataMapper
{
    public static ClienteDataModel ToDataModel(ClienteEntity e) => new()
    {
        IdCliente = e.IdCliente,
        ClienteGuid = e.ClienteGuid,
        TipoIdentificacion = e.TipoIdentificacion,
        NumeroIdentificacion = e.NumeroIdentificacion,
        Nombres = e.Nombres,
        Apellidos = e.Apellidos,
        RazonSocial = e.RazonSocial,
        Correo = e.Correo,
        Telefono = e.Telefono,
        Direccion = e.Direccion,
        Estado = e.Estado,
        EsEliminado = e.EsEliminado,
        CreadoPorUsuario = e.CreadoPorUsuario,
        FechaRegistroUtc = e.FechaRegistroUtc,
        ModificadoPorUsuario = e.ModificadoPorUsuario,
        FechaModificacionUtc = e.FechaModificacionUtc,
        ModificacionIp = e.ModificacionIp,
        FechaInhabilitacionUtc = e.FechaInhabilitacionUtc,
        MotivoInhabilitacion = e.MotivoInhabilitacion,
        ServicioOrigen = e.ServicioOrigen
    };

    public static void ApplyUpdate(ClienteEntity e, ClienteDataModel m)
    {
        e.TipoIdentificacion = m.TipoIdentificacion;
        e.NumeroIdentificacion = m.NumeroIdentificacion;
        e.Nombres = m.Nombres;
        e.Apellidos = m.Apellidos;
        e.RazonSocial = m.RazonSocial;
        e.Correo = m.Correo;
        e.Telefono = m.Telefono;
        e.Direccion = m.Direccion;
        e.ModificadoPorUsuario = m.ModificadoPorUsuario;
        e.FechaModificacionUtc = m.FechaModificacionUtc;
        e.ModificacionIp = m.ModificacionIp;
    }
}
