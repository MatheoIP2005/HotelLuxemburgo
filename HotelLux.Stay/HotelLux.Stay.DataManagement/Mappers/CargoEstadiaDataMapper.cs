using HotelLux.Stay.DataAccess.Entities;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Mappers;

public static class CargoEstadiaDataMapper
{
    public static CargoEstadiaDataModel ToModel(CargoEstadiaEntity e) => new()
    {
        IdCargoEstadia = e.IdCargoEstadia,
        CargoGuid = e.CargoGuid,
        IdEstadia = e.IdEstadia,
        EstadiaGuid = e.Estadia?.EstadiaGuid ?? Guid.Empty,
        CatalogoGuid = e.CatalogoGuid,
        DescripcionCargo = e.DescripcionCargo,
        Cantidad = e.Cantidad,
        PrecioUnitario = e.PrecioUnitario,
        Subtotal = e.Subtotal,
        ValorIva = e.ValorIva,
        TotalCargo = e.TotalCargo,
        FechaConsumoUtc = e.FechaConsumoUtc,
        EstadoCargo = e.EstadoCargo,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario
    };

    public static CargoEstadiaEntity ToEntity(CargoEstadiaDataModel m) => new()
    {
        IdCargoEstadia = m.IdCargoEstadia,
        CargoGuid = m.CargoGuid == Guid.Empty ? Guid.NewGuid() : m.CargoGuid,
        IdEstadia = m.IdEstadia,
        CatalogoGuid = m.CatalogoGuid,
        DescripcionCargo = m.DescripcionCargo,
        Cantidad = m.Cantidad,
        PrecioUnitario = m.PrecioUnitario,
        Subtotal = m.Subtotal,
        ValorIva = m.ValorIva,
        TotalCargo = m.TotalCargo,
        FechaConsumoUtc = m.FechaConsumoUtc,
        EstadoCargo = m.EstadoCargo,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario,
        ServicioOrigen = "stay-service"
    };
}
