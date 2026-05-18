using HotelLux.Reservation.DataAccess.Context;
using HotelLux.Reservation.DataAccess.Entities;
using HotelLux.Reservation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Reservation.DataAccess.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly ReservationDbContext _context;
    public ReservaRepository(ReservationDbContext context) => _context = context;

    public async Task<ReservaEntity?> ObtenerPorIdAsync(int idReserva, CancellationToken ct = default)
        => await _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.ReservasHabitaciones)
            .FirstOrDefaultAsync(x => x.IdReserva == idReserva && !x.EsEliminado, ct);

    public async Task<ReservaEntity?> ObtenerPorGuidAsync(Guid reservaGuid, CancellationToken ct = default)
        => await _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.ReservasHabitaciones)
            .FirstOrDefaultAsync(x => x.ReservaGuid == reservaGuid && !x.EsEliminado, ct);

    public async Task<ReservaEntity?> ObtenerPorCodigoAsync(string codigoReserva, CancellationToken ct = default)
        => await _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.ReservasHabitaciones)
            .FirstOrDefaultAsync(x => x.CodigoReserva == codigoReserva && !x.EsEliminado, ct);

    public async Task<ReservaEntity?> ObtenerParaActualizarAsync(int idReserva, CancellationToken ct = default)
        => await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.ReservasHabitaciones)
            .FirstOrDefaultAsync(x => x.IdReserva == idReserva && !x.EsEliminado, ct);

    public async Task<ReservaEntity?> ObtenerParaActualizarPorGuidAsync(Guid reservaGuid, CancellationToken ct = default)
        => await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.ReservasHabitaciones)
            .FirstOrDefaultAsync(x => x.ReservaGuid == reservaGuid && !x.EsEliminado, ct);

    public async Task<IReadOnlyList<ReservaEntity>> ListarAsync(CancellationToken ct = default)
        => await _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.ReservasHabitaciones)
            .Where(x => !x.EsEliminado)
            .OrderByDescending(x => x.FechaReservaUtc)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<ReservaEntity> Items, int Total)> BuscarAsync(
        Guid? clienteGuid, Guid? sucursalGuid, string? estadoReserva,
        DateOnly? fechaDesde, DateOnly? fechaHasta, string? origenCanal,
        int pagina, int limite, CancellationToken ct = default)
    {
        var query = _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Where(x => !x.EsEliminado);

        if (clienteGuid.HasValue)
            query = query.Where(x => x.Cliente != null && x.Cliente.ClienteGuid == clienteGuid.Value);
        if (sucursalGuid.HasValue)
            query = query.Where(x => x.SucursalGuid == sucursalGuid.Value);
        if (!string.IsNullOrWhiteSpace(estadoReserva))
            query = query.Where(x => x.EstadoReserva == estadoReserva.ToUpperInvariant());
        if (fechaDesde.HasValue)
            query = query.Where(x => x.FechaInicio >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(x => x.FechaFin <= fechaHasta.Value);
        if (!string.IsNullOrWhiteSpace(origenCanal))
            query = query.Where(x => x.OrigenCanalReserva == origenCanal);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.FechaReservaUtc)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .Include(r => r.ReservasHabitaciones)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AgregarAsync(ReservaEntity entity, CancellationToken ct = default)
        => await _context.Reservas.AddAsync(entity, ct);

    public void Actualizar(ReservaEntity entity) => _context.Reservas.Update(entity);
}
