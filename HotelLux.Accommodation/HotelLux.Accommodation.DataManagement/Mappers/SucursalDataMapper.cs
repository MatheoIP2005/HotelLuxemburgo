using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class SucursalDataMapper
{
    public static SucursalDataModel ToDataModel(SucursalEntity e) => new()
    {
        IdSucursal = e.IdSucursal,
        SucursalGuid = e.SucursalGuid,
        CodigoSucursal = e.CodigoSucursal,
        NombreSucursal = e.NombreSucursal,
        DescripcionSucursal = e.DescripcionSucursal,
        DescripcionCorta = e.DescripcionCorta,
        TipoAlojamiento = e.TipoAlojamiento,
        Estrellas = e.Estrellas,
        CategoriaViaje = e.CategoriaViaje,
        Pais = e.Pais,
        Provincia = e.Provincia,
        Ciudad = e.Ciudad,
        Ubicacion = e.Ubicacion,
        Direccion = e.Direccion,
        CodigoPostal = e.CodigoPostal,
        Telefono = e.Telefono,
        Correo = e.Correo,
        Latitud = e.Latitud,
        Longitud = e.Longitud,
        HoraCheckin = e.HoraCheckin,
        HoraCheckout = e.HoraCheckout,
        CheckinAnticipado = e.CheckinAnticipado,
        CheckoutTardio = e.CheckoutTardio,
        AceptaNinos = e.AceptaNinos,
        EdadMinimaHuesped = e.EdadMinimaHuesped,
        PermiteMascotas = e.PermiteMascotas,
        SePermiteFumar = e.SePermiteFumar,
        EstadoSucursal = e.EstadoSucursal,
        EsEliminado = e.EsEliminado,
        FechaInhabilitacionUtc = e.FechaInhabilitacionUtc,
        MotivoInhabilitacion = e.MotivoInhabilitacion,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario,
        ModificadoPorUsuario = e.ModificadoPorUsuario,
        FechaModificacionUtc = e.FechaModificacionUtc,
        ModificacionIp = e.ModificacionIp,
        ServicioOrigen = e.ServicioOrigen
    };

    public static SucursalEntity ToEntity(SucursalDataModel m) => new()
    {
        IdSucursal = m.IdSucursal,
        SucursalGuid = m.SucursalGuid,
        CodigoSucursal = m.CodigoSucursal,
        NombreSucursal = m.NombreSucursal,
        DescripcionSucursal = m.DescripcionSucursal,
        DescripcionCorta = m.DescripcionCorta,
        TipoAlojamiento = m.TipoAlojamiento,
        Estrellas = m.Estrellas,
        CategoriaViaje = m.CategoriaViaje,
        Pais = m.Pais,
        Provincia = m.Provincia,
        Ciudad = m.Ciudad,
        Ubicacion = m.Ubicacion,
        Direccion = m.Direccion,
        CodigoPostal = m.CodigoPostal,
        Telefono = m.Telefono,
        Correo = m.Correo,
        Latitud = m.Latitud,
        Longitud = m.Longitud,
        HoraCheckin = m.HoraCheckin,
        HoraCheckout = m.HoraCheckout,
        CheckinAnticipado = m.CheckinAnticipado,
        CheckoutTardio = m.CheckoutTardio,
        AceptaNinos = m.AceptaNinos,
        EdadMinimaHuesped = m.EdadMinimaHuesped,
        PermiteMascotas = m.PermiteMascotas,
        SePermiteFumar = m.SePermiteFumar,
        EstadoSucursal = m.EstadoSucursal,
        EsEliminado = m.EsEliminado,
        FechaInhabilitacionUtc = m.FechaInhabilitacionUtc,
        MotivoInhabilitacion = m.MotivoInhabilitacion,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario,
        ModificadoPorUsuario = m.ModificadoPorUsuario,
        FechaModificacionUtc = m.FechaModificacionUtc,
        ModificacionIp = m.ModificacionIp,
        ServicioOrigen = m.ServicioOrigen
    };
}
