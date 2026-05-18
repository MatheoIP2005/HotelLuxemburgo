using HotelLux.Auth.Business.DTOs.Roles;
using HotelLux.Auth.Business.DTOs.Usuarios;
using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.Business.Mappers;

public static class UsuarioBusinessMapper
{
    public static UsuarioDTO ToDTO(UsuarioDataModel model)
    {
        return new UsuarioDTO
        {
            IdUsuario = model.IdUsuario,
            UsuarioGuid = model.UsuarioGuid,
            ClienteGuid = model.ClienteGuid,
            Username = model.Username,
            Correo = model.Correo,
            Nombres = model.Nombres,
            Apellidos = model.Apellidos,
            EstadoUsuario = model.EstadoUsuario,
            Activo = model.Activo,
            FechaRegistroUtc = model.FechaRegistroUtc,
            CreadoPorUsuario = model.CreadoPorUsuario,
            Roles = model.Roles.ToList()
        };
    }

    public static UsuarioDataModel ToDataModel(UsuarioCreateDTO dto, string passwordHash, string passwordSalt)
    {
        return new UsuarioDataModel
        {
            UsuarioGuid = Guid.NewGuid(),
            ClienteGuid = dto.ClienteGuid,
            Username = dto.Username,
            Correo = dto.Correo,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            EstadoUsuario = "ACT",
            EsEliminado = false,
            Activo = true,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = dto.CreadoPorUsuario ?? "system",
            ModificacionIp = dto.CreadoDesdeIp
        };
    }

    public static UsuarioDataModel ToDataModel(UsuarioUpdateDTO dto, UsuarioDataModel existing)
    {
        existing.Username = dto.Username;
        existing.Correo = dto.Correo;
        existing.Nombres = dto.Nombres;
        existing.Apellidos = dto.Apellidos;
        existing.EstadoUsuario = dto.EstadoUsuario;
        existing.Activo = dto.EstadoUsuario == "ACT";
        existing.MotivoInhabilitacion = dto.MotivoInhabilitacion;
        existing.ModificadoPorUsuario = dto.ModificadoPorUsuario;
        existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existing.ModificacionIp = dto.ModificadoDesdeIp;
        return existing;
    }
}
