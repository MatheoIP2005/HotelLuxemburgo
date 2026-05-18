using HotelLux.Audit.DataAccess.Entities;

namespace HotelLux.Audit.DataAccess.Repositories.Interfaces;

public interface IEventoAuditoriaRepository
{
    Task AgregarAsync(EventoAuditoriaEntity entity, CancellationToken ct = default);
    Task<IReadOnlyList<EventoAuditoriaEntity>> ListarAsync(
        string? servicioOrigen,
        string? tablaAfectada,
        Guid? entidadGuid,
        string? usuarioEjecutor,
        int pagina,
        int limite,
        CancellationToken ct = default);
    Task<IReadOnlyList<EventoAuditoriaEntity>> ListarPorEntidadAsync(Guid entidadGuid, CancellationToken ct = default);
    Task<IReadOnlyList<EventoAuditoriaEntity>> ListarPorServicioAsync(string servicioOrigen, CancellationToken ct = default);
    Task<EventoAuditoriaEntity?> ObtenerPorAuditoriaGuidAsync(Guid auditoriaGuid, CancellationToken ct = default);
}
