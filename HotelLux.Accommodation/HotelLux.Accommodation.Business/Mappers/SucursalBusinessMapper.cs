using HotelLux.Accommodation.Business.DTOs.Sucursal;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.Business.Mappers;

public static class SucursalBusinessMapper
{
    public static SucursalDTO ToDTO(SucursalDataModel m) => new()
    {
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
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };

    public static SucursalDataModel ToDataModel(SucursalCreateDTO dto) => new()
    {
        CodigoSucursal = dto.CodigoSucursal,
        NombreSucursal = dto.NombreSucursal,
        DescripcionSucursal = dto.DescripcionSucursal,
        DescripcionCorta = dto.DescripcionCorta,
        TipoAlojamiento = dto.TipoAlojamiento,
        Estrellas = dto.Estrellas,
        CategoriaViaje = dto.CategoriaViaje,
        Pais = dto.Pais,
        Provincia = dto.Provincia,
        Ciudad = dto.Ciudad,
        Ubicacion = dto.Ubicacion,
        Direccion = dto.Direccion,
        CodigoPostal = dto.CodigoPostal,
        Telefono = dto.Telefono,
        Correo = dto.Correo,
        Latitud = dto.Latitud,
        Longitud = dto.Longitud,
        HoraCheckin = dto.HoraCheckin,
        HoraCheckout = dto.HoraCheckout,
        CheckinAnticipado = dto.CheckinAnticipado,
        CheckoutTardio = dto.CheckoutTardio,
        AceptaNinos = dto.AceptaNinos,
        EdadMinimaHuesped = dto.EdadMinimaHuesped,
        PermiteMascotas = dto.PermiteMascotas,
        SePermiteFumar = dto.SePermiteFumar,
        EstadoSucursal = "ACT",
        FechaRegistroUtc = DateTimeOffset.UtcNow,
        CreadoPorUsuario = dto.CreadoPorUsuario ?? "system",
        ModificacionIp = dto.CreadoDesdeIp,
        ServicioOrigen = "accommodation-service"
    };

    public static SucursalDataModel ToDataModel(SucursalUpdateDTO dto, SucursalDataModel existing)
    {
        existing.CodigoSucursal = dto.CodigoSucursal;
        existing.NombreSucursal = dto.NombreSucursal;
        existing.DescripcionSucursal = dto.DescripcionSucursal;
        existing.DescripcionCorta = dto.DescripcionCorta;
        existing.TipoAlojamiento = dto.TipoAlojamiento;
        existing.Estrellas = dto.Estrellas;
        existing.CategoriaViaje = dto.CategoriaViaje;
        existing.Pais = dto.Pais;
        existing.Provincia = dto.Provincia;
        existing.Ciudad = dto.Ciudad;
        existing.Ubicacion = dto.Ubicacion;
        existing.Direccion = dto.Direccion;
        existing.CodigoPostal = dto.CodigoPostal;
        existing.Telefono = dto.Telefono;
        existing.Correo = dto.Correo;
        existing.Latitud = dto.Latitud;
        existing.Longitud = dto.Longitud;
        existing.HoraCheckin = dto.HoraCheckin;
        existing.HoraCheckout = dto.HoraCheckout;
        existing.CheckinAnticipado = dto.CheckinAnticipado;
        existing.CheckoutTardio = dto.CheckoutTardio;
        existing.AceptaNinos = dto.AceptaNinos;
        existing.EdadMinimaHuesped = dto.EdadMinimaHuesped;
        existing.PermiteMascotas = dto.PermiteMascotas;
        existing.SePermiteFumar = dto.SePermiteFumar;
        existing.EstadoSucursal = dto.EstadoSucursal;
        existing.ModificadoPorUsuario = dto.ModificadoPorUsuario;
        existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existing.ModificacionIp = dto.ModificadoDesdeIp;
        return existing;
    }
}
