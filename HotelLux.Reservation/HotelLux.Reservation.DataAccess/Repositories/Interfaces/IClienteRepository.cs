using HotelLux.Reservation.DataAccess.Entities;

namespace HotelLux.Reservation.DataAccess.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<ClienteEntity?> ObtenerPorIdAsync(int idCliente, CancellationToken ct = default);
    Task<ClienteEntity?> ObtenerPorGuidAsync(Guid clienteGuid, CancellationToken ct = default);
    Task<ClienteEntity?> ObtenerParaActualizarAsync(Guid clienteGuid, CancellationToken ct = default);
    Task<ClienteEntity?> ObtenerPorIdentificacionAsync(
        string tipoId, string numeroId, CancellationToken ct = default);
    Task<(IReadOnlyList<ClienteEntity> Items, int Total)> ListarAsync(int pagina, int limite, CancellationToken ct = default);
    Task<bool> ExisteCorreoAsync(string correo, Guid? exceptoClienteGuid, CancellationToken ct = default);
    Task AgregarAsync(ClienteEntity entity, CancellationToken ct = default);
    void Actualizar(ClienteEntity entity);
    Task<bool> ExisteAsync(Guid clienteGuid, CancellationToken ct = default);
}
