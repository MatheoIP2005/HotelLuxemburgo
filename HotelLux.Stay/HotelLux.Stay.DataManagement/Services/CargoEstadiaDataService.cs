using HotelLux.Stay.DataManagement.Interfaces;
using HotelLux.Stay.DataManagement.Mappers;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Services;

public class CargoEstadiaDataService : ICargoEstadiaDataService
{
    private readonly IUnitOfWork _uow;
    public CargoEstadiaDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<CargoEstadiaDataModel>> ListarPorEstadiaAsync(
        Guid estadiaGuid, CancellationToken ct = default)
    {
        var estadia = await _uow.EstadiaRepository.ObtenerPorGuidAsync(estadiaGuid, ct)
            ?? throw new InvalidOperationException($"Estadía '{estadiaGuid}' no existe.");
        var list = await _uow.CargoEstadiaRepository.ListarPorEstadiaAsync(estadia.IdEstadia, ct);
        return list.Select(x =>
        {
            var model = CargoEstadiaDataMapper.ToModel(x);
            model.EstadiaGuid = estadia.EstadiaGuid;
            return model;
        }).ToList();
    }

    public async Task<CargoEstadiaDataModel> CrearAsync(CargoEstadiaDataModel model, CancellationToken ct = default)
    {
        var e = CargoEstadiaDataMapper.ToEntity(model);
        if (e.CargoGuid == Guid.Empty)
            e.CargoGuid = Guid.NewGuid();
        e.EstadoCargo = string.IsNullOrWhiteSpace(e.EstadoCargo) ? "PEN" : e.EstadoCargo;
        e.FechaConsumoUtc = e.FechaConsumoUtc == default ? DateTimeOffset.UtcNow : e.FechaConsumoUtc;
        e.FechaRegistroUtc = DateTimeOffset.UtcNow;
        await _uow.CargoEstadiaRepository.AgregarAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        var saved = await _uow.CargoEstadiaRepository.ObtenerPorGuidAsync(e.CargoGuid, ct) ?? e;
        var result = CargoEstadiaDataMapper.ToModel(saved);
        result.EstadiaGuid = model.EstadiaGuid;
        return result;
    }

    public async Task<CargoEstadiaDataModel?> ObtenerPorCargoGuidAsync(Guid cargoGuid, CancellationToken ct = default)
    {
        var e = await _uow.CargoEstadiaRepository.ObtenerPorGuidAsync(cargoGuid, ct);
        if (e is null) return null;
        var est = await _uow.EstadiaRepository.ObtenerPorIdAsync(e.IdEstadia, ct);
        if (est is null) return null;
        var model = CargoEstadiaDataMapper.ToModel(e);
        model.EstadiaGuid = est.EstadiaGuid;
        return model;
    }

    public async Task AnularCargoAsync(Guid cargoGuid, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.CargoEstadiaRepository.ObtenerParaActualizarPorGuidAsync(cargoGuid, ct)
            ?? throw new InvalidOperationException($"Cargo '{cargoGuid}' no encontrado.");
        if (e.EstadoCargo == "ANU")
            return;
        if (e.EstadoCargo != "PEN")
            throw new InvalidOperationException("Solo se pueden anular cargos en estado PEN.");
        e.EstadoCargo = "ANU";
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.CargoEstadiaRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
    }
}
