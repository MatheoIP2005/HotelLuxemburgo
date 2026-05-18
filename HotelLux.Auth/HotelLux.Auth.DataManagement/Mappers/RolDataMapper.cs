using HotelLux.Auth.DataAccess.Entities;
using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.DataManagement.Mappers;

public static class RolDataMapper
{
    public static RolDataModel ToDataModel(RolEntity entity)
    {
        return new RolDataModel
        {
            IdRol = entity.IdRol,
            RolGuid = entity.RolGuid,
            NombreRol = entity.NombreRol,
            DescripcionRol = entity.DescripcionRol,
            EstadoRol = entity.EstadoRol,
            EsEliminado = entity.EsEliminado,
            Activo = entity.Activo,
            FechaInhabilitacionUtc = entity.FechaInhabilitacionUtc,
            MotivoInhabilitacion = entity.MotivoInhabilitacion,
            FechaRegistroUtc = entity.FechaRegistroUtc,
            CreadoPorUsuario = entity.CreadoPorUsuario,
            ModificadoPorUsuario = entity.ModificadoPorUsuario,
            FechaModificacionUtc = entity.FechaModificacionUtc,
            ModificacionIp = entity.ModificacionIp
        };
    }

    public static RolEntity ToEntity(RolDataModel model)
    {
        return new RolEntity
        {
            IdRol = model.IdRol,
            RolGuid = model.RolGuid,
            NombreRol = model.NombreRol,
            DescripcionRol = model.DescripcionRol,
            EstadoRol = model.EstadoRol,
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
}
