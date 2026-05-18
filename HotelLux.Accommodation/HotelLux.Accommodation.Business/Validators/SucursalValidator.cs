using HotelLux.Accommodation.Business.DTOs.Sucursal;

namespace HotelLux.Accommodation.Business.Validators;

public static class SucursalValidator
{
    public static List<string> ValidarCreacion(SucursalCreateDTO dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.CodigoSucursal)) errors.Add("CodigoSucursal es requerido.");
        if (string.IsNullOrWhiteSpace(dto.NombreSucursal)) errors.Add("NombreSucursal es requerido.");
        if (string.IsNullOrWhiteSpace(dto.Pais)) errors.Add("Pais es requerido.");
        if (string.IsNullOrWhiteSpace(dto.Ciudad)) errors.Add("Ciudad es requerido.");
        if (string.IsNullOrWhiteSpace(dto.Direccion)) errors.Add("Direccion es requerido.");
        if (string.IsNullOrWhiteSpace(dto.Telefono)) errors.Add("Telefono es requerido.");
        if (string.IsNullOrWhiteSpace(dto.Correo)) errors.Add("Correo es requerido.");
        if (string.IsNullOrWhiteSpace(dto.Ubicacion)) errors.Add("Ubicacion es requerido.");
        if (string.IsNullOrWhiteSpace(dto.TipoAlojamiento)) errors.Add("TipoAlojamiento es requerido.");
        return errors;
    }

    public static List<string> ValidarActualizacion(SucursalUpdateDTO dto) =>
        ValidarCreacion(new SucursalCreateDTO
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
            SePermiteFumar = dto.SePermiteFumar
        });
}
