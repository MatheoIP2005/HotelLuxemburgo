using HotelLux.Auth.Business.DTOs.Roles;
using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.Business.Mappers;

public static class RolBusinessMapper
{
    public static RolDTO ToDTO(RolDataModel model)
    {
        return new RolDTO
        {
            IdRol = model.IdRol,
            RolGuid = model.RolGuid,
            NombreRol = model.NombreRol,
            DescripcionRol = model.DescripcionRol,
            EstadoRol = model.EstadoRol,
            Activo = model.Activo,
            FechaRegistroUtc = model.FechaRegistroUtc,
            CreadoPorUsuario = model.CreadoPorUsuario
        };
    }

    public static RolDataModel ToDataModel(RolCreateDTO dto)
    {
        return new RolDataModel
        {
            RolGuid = Guid.NewGuid(),
            NombreRol = dto.NombreRol,
            DescripcionRol = dto.DescripcionRol,
            EstadoRol = "ACT",
            EsEliminado = false,
            Activo = true,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = dto.CreadoPorUsuario ?? "system",
            ModificacionIp = dto.CreadoDesdeIp
        };
    }

    public static RolDataModel ToDataModel(RolUpdateDTO dto, RolDataModel existing)
    {
        existing.NombreRol = dto.NombreRol;
        existing.DescripcionRol = dto.DescripcionRol;
        existing.EstadoRol = dto.EstadoRol;
        existing.Activo = dto.EstadoRol == "ACT";
        existing.ModificadoPorUsuario = dto.ModificadoPorUsuario;
        existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existing.ModificacionIp = dto.ModificadoDesdeIp;
        return existing;
    }
}
