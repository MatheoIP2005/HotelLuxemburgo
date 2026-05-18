using HotelLux.Reservation.DataAccess.Context;
using HotelLux.Reservation.DataAccess.Entities;
using HotelLux.Reservation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Reservation.DataAccess.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ReservationDbContext _context;
    public ClienteRepository(ReservationDbContext context) => _context = context;

    public async Task<ClienteEntity?> ObtenerPorIdAsync(int idCliente, CancellationToken ct = default)
        => await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && !x.EsEliminado, ct);

    public async Task<ClienteEntity?> ObtenerPorGuidAsync(Guid clienteGuid, CancellationToken ct = default)
        => await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClienteGuid == clienteGuid && !x.EsEliminado, ct);

    public async Task<ClienteEntity?> ObtenerParaActualizarAsync(Guid clienteGuid, CancellationToken ct = default)
        => await _context.Clientes
            .FirstOrDefaultAsync(x => x.ClienteGuid == clienteGuid && !x.EsEliminado, ct);

    public async Task<ClienteEntity?> ObtenerPorIdentificacionAsync(
        string tipoId, string numeroId, CancellationToken ct = default)
        => await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TipoIdentificacion == tipoId &&
                x.NumeroIdentificacion == numeroId &&
                !x.EsEliminado, ct);

    public async Task<(IReadOnlyList<ClienteEntity> Items, int Total)> ListarAsync(int pagina, int limite, CancellationToken ct = default)
    {
        var q = _context.Clientes.AsNoTracking().Where(x => !x.EsEliminado);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.FechaRegistroUtc)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<bool> ExisteCorreoAsync(string correo, Guid? exceptoClienteGuid, CancellationToken ct = default)
    {
        var c = correo.Trim().ToLowerInvariant();
        var q = _context.Clientes.AsNoTracking().Where(x => !x.EsEliminado && x.Correo.ToLower() == c);
        if (exceptoClienteGuid.HasValue)
            q = q.Where(x => x.ClienteGuid != exceptoClienteGuid.Value);
        return await q.AnyAsync(ct);
    }

    public async Task AgregarAsync(ClienteEntity entity, CancellationToken ct = default)
        => await _context.Clientes.AddAsync(entity, ct);

    public void Actualizar(ClienteEntity entity) => _context.Clientes.Update(entity);

    public async Task<bool> ExisteAsync(Guid clienteGuid, CancellationToken ct = default)
        => await _context.Clientes.AsNoTracking()
            .AnyAsync(x => x.ClienteGuid == clienteGuid && !x.EsEliminado, ct);
}
