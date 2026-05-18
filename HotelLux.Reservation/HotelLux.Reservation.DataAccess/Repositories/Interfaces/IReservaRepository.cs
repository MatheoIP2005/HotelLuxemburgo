using HotelLux.Reservation.DataAccess.Entities;

namespace HotelLux.Reservation.DataAccess.Repositories.Interfaces;

public interface IReservaRepository
{
    Task<ReservaEntity?> ObtenerPorIdAsync(int idReserva, CancellationToken ct = default);
    Task<ReservaEntity?> ObtenerPorGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<ReservaEntity?> ObtenerPorCodigoAsync(string codigoReserva, CancellationToken ct = default);
    Task<ReservaEntity?> ObtenerParaActualizarAsync(int idReserva, CancellationToken ct = default);
    Task<ReservaEntity?> ObtenerParaActualizarPorGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaEntity>> ListarAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<ReservaEntity> Items, int Total)> BuscarAsync(
        Guid? clienteGuid, Guid? sucursalGuid, string? estadoReserva,
        DateOnly? fechaDesde, DateOnly? fechaHasta, string? origenCanal,
        int pagina, int limite, CancellationToken ct = default);
    Task AgregarAsync(ReservaEntity entity, CancellationToken ct = default);
    void Actualizar(ReservaEntity entity);
}
