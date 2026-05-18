using HotelLux.Reservation.Business.DTOs.Cliente;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.Business.Mappers;

public static class ClienteBusinessMapper
{
    public static ClienteDto ToDto(ClienteDataModel m) => new()
    {
        ClienteGuid = m.ClienteGuid,
        TipoIdentificacion = m.TipoIdentificacion,
        NumeroIdentificacion = m.NumeroIdentificacion,
        Nombres = m.Nombres,
        Apellidos = m.Apellidos,
        RazonSocial = m.RazonSocial,
        Correo = m.Correo,
        Telefono = m.Telefono,
        Direccion = m.Direccion,
        Estado = m.Estado,
        FechaRegistroUtc = m.FechaRegistroUtc
    };

    public static ClienteDataModel ToDataModel(ClienteCreateDto dto, string creadoPor) => new()
    {
        TipoIdentificacion = dto.TipoIdentificacion,
        NumeroIdentificacion = dto.NumeroIdentificacion,
        Nombres = dto.Nombres,
        Apellidos = dto.Apellidos,
        RazonSocial = dto.RazonSocial,
        Correo = dto.Correo,
        Telefono = dto.Telefono,
        Direccion = dto.Direccion,
        CreadoPorUsuario = creadoPor
    };

    public static ClienteDataModel ToDataModel(Guid clienteGuid, ClienteUpdateDto dto, string modificadoPor) => new()
    {
        ClienteGuid = clienteGuid,
        TipoIdentificacion = dto.TipoIdentificacion,
        NumeroIdentificacion = dto.NumeroIdentificacion,
        Nombres = dto.Nombres,
        Apellidos = dto.Apellidos,
        RazonSocial = dto.RazonSocial,
        Correo = dto.Correo,
        Telefono = dto.Telefono,
        Direccion = dto.Direccion,
        ModificadoPorUsuario = modificadoPor,
        FechaModificacionUtc = DateTimeOffset.UtcNow
    };
}
