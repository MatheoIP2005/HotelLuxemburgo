using HotelLux.Auth.DataAccess.Entities;
using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.DataManagement.Mappers;

public static class UsuarioDataMapper
{
    public static UsuarioDataModel ToDataModel(UsuarioAppEntity entity)
    {
        return new UsuarioDataModel
        {
            IdUsuario = entity.IdUsuario,
            UsuarioGuid = entity.UsuarioGuid,
            ClienteGuid = entity.ClienteGuid,
            Username = entity.Username,
            Correo = entity.Correo,
            Nombres = entity.Nombres,
            Apellidos = entity.Apellidos,
            PasswordHash = entity.PasswordHash,
            PasswordSalt = entity.PasswordSalt,
            EstadoUsuario = entity.EstadoUsuario,
            EsEliminado = entity.EsEliminado,
            Activo = entity.Activo,
            FechaInhabilitacionUtc = entity.FechaInhabilitacionUtc,
            MotivoInhabilitacion = entity.MotivoInhabilitacion,
            FechaRegistroUtc = entity.FechaRegistroUtc,
            CreadoPorUsuario = entity.CreadoPorUsuario,
            ModificadoPorUsuario = entity.ModificadoPorUsuario,
            FechaModificacionUtc = entity.FechaModificacionUtc,
            ModificacionIp = entity.ModificacionIp,
            Roles = entity.UsuarioRoles
                .Select(ur => ur.Rol.NombreRol)
                .Distinct()
                .ToList()
        };
    }

    public static UsuarioAppEntity ToEntity(UsuarioDataModel model)
    {
        return new UsuarioAppEntity
        {
            IdUsuario = model.IdUsuario,
            UsuarioGuid = model.UsuarioGuid,
            ClienteGuid = model.ClienteGuid,
            Username = model.Username,
            Correo = model.Correo,
            Nombres = model.Nombres,
            Apellidos = model.Apellidos,
            PasswordHash = model.PasswordHash,
            PasswordSalt = model.PasswordSalt,
            EstadoUsuario = model.EstadoUsuario,
            EsEliminado = model.EsEliminado,
            Activo = model.Activo,
            FechaInhabilitacionUtc = model.FechaInhabilitacionUtc,
            MotivoInhabilitacion = model.MotivoInhabilitacion,
            FechaRegistroUtc = model.FechaRegistroUtc,
            CreadoPorUsuario = model.CreadoPorUsuario,
            ModificadoPorUsuario = model.ModificadoPorUsuario,
            FechaModificacionUtc = model.FechaModificacionUtc,
            ModificacionIp = model.ModificacionIp
        };
    }

    public static LoginDataModel ToLoginDataModel(UsuarioAppEntity entity)
    {
        return new LoginDataModel
        {
            Username = entity.Username,
            PasswordHash = entity.PasswordHash,
            PasswordSalt = entity.PasswordSalt,
            Activo = entity.Activo,
            EsEliminado = entity.EsEliminado,
            EstadoUsuario = entity.EstadoUsuario,
            Nombres = entity.Nombres,
            Apellidos = entity.Apellidos,
            Correo = entity.Correo,
            UsuarioGuid = entity.UsuarioGuid,
            ClienteGuid = entity.ClienteGuid,
            Roles = entity.UsuarioRoles
                .Where(ur => !ur.EsEliminado && ur.Activo && ur.EstadoUsuarioRol == "ACT" && !ur.Rol.EsEliminado && ur.Rol.Activo && ur.Rol.EstadoRol == "ACT")
                .Select(ur => ur.Rol.NombreRol)
                .Distinct()
                .ToList()
        };
    }
}
