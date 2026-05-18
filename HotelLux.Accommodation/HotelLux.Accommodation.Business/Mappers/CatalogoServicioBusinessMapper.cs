using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.Business.Mappers;

public static class CatalogoServicioBusinessMapper
{
    public static CatalogoServicioDTO ToDTO(CatalogoServicioDataModel m) => new()
    {
        CatalogoGuid = m.CatalogoGuid,
        SucursalGuid = m.SucursalGuid,
        CodigoCatalogo = m.CodigoCatalogo,
        NombreCatalogo = m.NombreCatalogo,
        TipoCatalogo = m.TipoCatalogo,
        CategoriaCatalogo = m.CategoriaCatalogo,
        DescripcionCatalogo = m.DescripcionCatalogo,
        PrecioBase = m.PrecioBase,
        AplicaIva = m.AplicaIva,
        Disponible24h = m.Disponible24h,
        HoraInicio = m.HoraInicio,
        HoraFin = m.HoraFin,
        IconoUrl = m.IconoUrl,
        EstadoCatalogo = m.EstadoCatalogo,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };

    public static CatalogoServicioDataModel ToDataModel(CatalogoServicioCreateDTO dto, int? idSucursal) => new()
    {
        IdSucursal = idSucursal,
        CodigoCatalogo = dto.CodigoCatalogo,
        NombreCatalogo = dto.NombreCatalogo,
        TipoCatalogo = dto.TipoCatalogo,
        CategoriaCatalogo = dto.CategoriaCatalogo,
        DescripcionCatalogo = dto.DescripcionCatalogo,
        PrecioBase = dto.PrecioBase,
        AplicaIva = dto.AplicaIva,
        Disponible24h = dto.Disponible24h,
        HoraInicio = dto.HoraInicio,
        HoraFin = dto.HoraFin,
        IconoUrl = dto.IconoUrl,
        EstadoCatalogo = "ACT",
        FechaRegistroUtc = DateTimeOffset.UtcNow,
        CreadoPorUsuario = dto.CreadoPorUsuario ?? "system",
        ModificacionIp = dto.CreadoDesdeIp,
        ServicioOrigen = "accommodation-service"
    };

    public static CatalogoServicioDataModel ToDataModel(CatalogoServicioUpdateDTO dto, CatalogoServicioDataModel existing, int? idSucursal)
    {
        existing.IdSucursal = idSucursal;
        existing.NombreCatalogo = dto.NombreCatalogo;
        existing.TipoCatalogo = dto.TipoCatalogo;
        existing.CategoriaCatalogo = dto.CategoriaCatalogo;
        existing.DescripcionCatalogo = dto.DescripcionCatalogo;
        existing.PrecioBase = dto.PrecioBase;
        existing.AplicaIva = dto.AplicaIva;
        existing.Disponible24h = dto.Disponible24h;
        existing.HoraInicio = dto.HoraInicio;
        existing.HoraFin = dto.HoraFin;
        existing.IconoUrl = dto.IconoUrl;
        existing.EstadoCatalogo = dto.EstadoCatalogo;
        existing.ModificadoPorUsuario = dto.ModificadoPorUsuario;
        existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existing.ModificacionIp = dto.ModificadoDesdeIp;
        return existing;
    }
}
