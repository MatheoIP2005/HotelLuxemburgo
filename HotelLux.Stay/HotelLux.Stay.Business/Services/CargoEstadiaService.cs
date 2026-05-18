using HotelLux.Stay.Business.DTOs;
using HotelLux.Stay.Business.Exceptions;
using HotelLux.Stay.Business.Interfaces;
using HotelLux.Stay.DataManagement.Interfaces;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.Business.Services;

public class CargoEstadiaService : ICargoEstadiaService
{
    private readonly ICargoEstadiaDataService _cargoData;
    private readonly IEstadiaDataService _estadiaData;

    public CargoEstadiaService(ICargoEstadiaDataService cargoData, IEstadiaDataService estadiaData)
    {
        _cargoData = cargoData;
        _estadiaData = estadiaData;
    }

    public async Task<CargoEstadiaDto> CrearAsync(
        Guid estadiaGuid, CargoEstadiaCreateDto dto, CancellationToken ct = default)
    {
        var estadia = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, ct);
        if (estadia is null)
            throw new NotFoundException("Estadía", estadiaGuid);
        if (estadia.Estado != "ACT")
            throw new ConflictException("Cargo de estadía", "Solo se pueden agregar cargos a estadías activas.");

        var subtotal = dto.Cantidad * dto.PrecioUnitario;
        var model = new CargoEstadiaDataModel
        {
            IdEstadia = estadia.IdEstadia,
            EstadiaGuid = estadia.EstadiaGuid,
            CatalogoGuid = dto.CatalogoGuid,
            DescripcionCargo = dto.DescripcionCargo.Trim(),
            Cantidad = dto.Cantidad,
            PrecioUnitario = dto.PrecioUnitario,
            Subtotal = subtotal,
            ValorIva = dto.ValorIva,
            TotalCargo = subtotal + dto.ValorIva,
            FechaConsumoUtc = DateTimeOffset.UtcNow,
            EstadoCargo = "PEN",
            CreadoPorUsuario = dto.CreadoPorUsuario ?? "stay_api"
        };

        var created = await _cargoData.CrearAsync(model, ct);
        return ToDto(created);
    }

    public async Task<IReadOnlyList<CargoEstadiaDto>> ListarPorEstadiaAsync(
        Guid estadiaGuid, CancellationToken ct = default)
    {
        var estadia = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, ct);
        if (estadia is null)
            throw new NotFoundException("Estadía", estadiaGuid);
        var list = await _cargoData.ListarPorEstadiaAsync(estadiaGuid, ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<CargoEstadiaDto> ObtenerPorGuidAsync(Guid cargoGuid, CancellationToken ct = default)
    {
        var m = await _cargoData.ObtenerPorCargoGuidAsync(cargoGuid, ct);
        if (m is null)
            throw new NotFoundException("Cargo de estadía", cargoGuid);
        return ToDto(m);
    }

    public async Task AnularAsync(Guid cargoGuid, string usuario, CancellationToken ct = default)
    {
        try
        {
            await _cargoData.AnularCargoAsync(cargoGuid, usuario, ct);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                throw new NotFoundException("Cargo de estadía", cargoGuid);
            throw new ConflictException("Cargo de estadía", ex.Message);
        }
    }

    private static CargoEstadiaDto ToDto(CargoEstadiaDataModel m) => new()
    {
        CargoGuid = m.CargoGuid,
        EstadiaGuid = m.EstadiaGuid,
        CatalogoGuid = m.CatalogoGuid,
        DescripcionCargo = m.DescripcionCargo,
        Cantidad = m.Cantidad,
        PrecioUnitario = m.PrecioUnitario,
        Subtotal = m.Subtotal,
        ValorIva = m.ValorIva,
        TotalCargo = m.TotalCargo,
        FechaConsumoUtc = m.FechaConsumoUtc,
        EstadoCargo = m.EstadoCargo
    };
}
