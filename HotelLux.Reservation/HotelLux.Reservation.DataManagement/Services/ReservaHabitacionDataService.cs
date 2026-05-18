using HotelLux.Reservation.DataManagement.Interfaces;
using HotelLux.Reservation.DataManagement.Mappers;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Services;

public class ReservaHabitacionDataService : IReservaHabitacionDataService
{
    private readonly IUnitOfWork _uow;
    public ReservaHabitacionDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ReservaHabitacionDataModel>> ListarPorReservaAsync(int idReserva, CancellationToken ct = default)
    {
        var list = await _uow.ReservaHabitacionRepository.ListarPorReservaAsync(idReserva, ct);
        return list.Select(ReservaHabitacionDataMapper.ToDataModel).ToList();
    }

    public async Task<ReservaHabitacionDataModel?> ObtenerPorGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default)
    {
        var e = await _uow.ReservaHabitacionRepository.ObtenerPorGuidAsync(reservaHabitacionGuid, ct);
        return e is null ? null : ReservaHabitacionDataMapper.ToDataModel(e);
    }

    public async Task ActualizarEstadoAsync(Guid reservaHabitacionGuid, string nuevoEstado, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.ReservaHabitacionRepository.ObtenerParaActualizarPorGuidAsync(reservaHabitacionGuid, ct);
        if (e is null) return;
        e.EstadoDetalle = nuevoEstado;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.ReservaHabitacionRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<ReservaHabitacionDataModel> InsertarLineaAsync(
        int idReserva, ReservaHabitacionDataModel line, CancellationToken ct = default)
    {
        var e = ReservaHabitacionDataMapper.ToEntity(line);
        e.IdReserva = idReserva;
        e.IdReservaHabitacion = 0;
        if (e.ReservaHabitacionGuid == Guid.Empty)
            e.ReservaHabitacionGuid = Guid.NewGuid();
        e.FechaRegistroUtc = DateTimeOffset.UtcNow;
        await _uow.ReservaHabitacionRepository.AgregarAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        var saved = await _uow.ReservaHabitacionRepository.ObtenerPorGuidAsync(e.ReservaHabitacionGuid, ct)
            ?? throw new InvalidOperationException("No se pudo recargar la línea de habitación creada.");
        return ReservaHabitacionDataMapper.ToDataModel(saved);
    }

    public async Task<bool> EliminarLineaAsync(int idReserva, Guid reservaHabitacionGuid, CancellationToken ct = default)
    {
        var e = await _uow.ReservaHabitacionRepository.ObtenerParaActualizarPorGuidAsync(reservaHabitacionGuid, ct);
        if (e is null || e.IdReserva != idReserva)
            return false;
        _uow.ReservaHabitacionRepository.Eliminar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
