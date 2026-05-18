using HotelLux.Stay.DataAccess.Entities;

namespace HotelLux.Stay.DataAccess.Repositories.Interfaces;

public interface IEstadiaRepository
{
    Task<EstadiaEntity?> ObtenerPorGuidAsync(Guid estadiaGuid, CancellationToken ct = default);
    Task<EstadiaEntity?> ObtenerPorIdAsync(int idEstadia, CancellationToken ct = default);
    Task<EstadiaEntity?> ObtenerParaActualizarAsync(Guid estadiaGuid, CancellationToken ct = default);
    Task<EstadiaEntity?> ObtenerActivaPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<EstadiaEntity?> ObtenerActivaPorReservaHabitacionGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default);
    Task<(IReadOnlyList<EstadiaEntity> Items, int Total)> ListarAsync(
        string? estado, Guid? sucursalGuid, int pagina, int limite, CancellationToken ct = default);
    Task AgregarAsync(EstadiaEntity entity, CancellationToken ct = default);
    void Actualizar(EstadiaEntity entity);
}
