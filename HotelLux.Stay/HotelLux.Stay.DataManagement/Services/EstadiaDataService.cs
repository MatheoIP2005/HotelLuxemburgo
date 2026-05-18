using HotelLux.Stay.DataManagement.Interfaces;
using HotelLux.Stay.DataManagement.Mappers;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Services;

public class EstadiaDataService : IEstadiaDataService
{
    private readonly IUnitOfWork _uow;
    public EstadiaDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<EstadiaDataModel?> ObtenerPorGuidAsync(Guid estadiaGuid, CancellationToken ct = default)
    {
        var e = await _uow.EstadiaRepository.ObtenerPorGuidAsync(estadiaGuid, ct);
        return e is null ? null : EstadiaDataMapper.ToModel(e);
    }

    public async Task<EstadiaDataModel?> ObtenerActivaPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default)
    {
        var e = await _uow.EstadiaRepository.ObtenerActivaPorReservaGuidAsync(reservaGuid, ct);
        return e is null ? null : EstadiaDataMapper.ToModel(e);
    }

    public async Task<EstadiaDataModel?> ObtenerActivaPorReservaHabitacionGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default)
    {
        var e = await _uow.EstadiaRepository.ObtenerActivaPorReservaHabitacionGuidAsync(reservaHabitacionGuid, ct);
        return e is null ? null : EstadiaDataMapper.ToModel(e);
    }

    public async Task<EstadiaDataModel> CrearAsync(EstadiaDataModel model, CancellationToken ct = default)
    {
        var e = EstadiaDataMapper.ToEntity(model);
        if (e.EstadiaGuid == Guid.Empty)
            e.EstadiaGuid = Guid.NewGuid();
        e.FechaRegistroUtc = DateTimeOffset.UtcNow;
        e.ServicioOrigen = "stay-service";
        await _uow.EstadiaRepository.AgregarAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.EstadiaRepository.ObtenerPorGuidAsync(e.EstadiaGuid, ct) ?? e;
        return EstadiaDataMapper.ToModel(reloaded);
    }

    public async Task<EstadiaDataModel?> ActualizarAsync(EstadiaDataModel model, CancellationToken ct = default)
    {
        var e = await _uow.EstadiaRepository.ObtenerParaActualizarAsync(model.EstadiaGuid, ct);
        if (e is null) return null;
        e.Estado = model.Estado;
        e.FechaCheckinUtc = model.FechaCheckinUtc;
        e.FechaCheckoutUtc = model.FechaCheckoutUtc;
        e.RequiereMantenimiento = model.RequiereMantenimiento;
        e.ModificadoPorUsuario = model.ModificadoPorUsuario;
        e.FechaModificacionUtc = model.FechaModificacionUtc;
        _uow.EstadiaRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        var r = await _uow.EstadiaRepository.ObtenerPorGuidAsync(model.EstadiaGuid, ct);
        return r is null ? null : EstadiaDataMapper.ToModel(r);
    }

    public async Task<(IReadOnlyList<EstadiaDataModel> Items, int Total)> ListarAsync(
        string? estado, Guid? sucursalGuid, int pagina, int limite, CancellationToken ct = default)
    {
        var (items, total) = await _uow.EstadiaRepository.ListarAsync(estado, sucursalGuid, pagina, limite, ct);
        return (items.Select(EstadiaDataMapper.ToModel).ToList(), total);
    }
}
