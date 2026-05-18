using HotelLux.Reservation.DataAccess.Context;
using HotelLux.Reservation.DataAccess.Entities;
using HotelLux.Reservation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Reservation.DataAccess.Repositories;

public class ReservaHabitacionRepository : IReservaHabitacionRepository
{
    private readonly ReservationDbContext _context;
    public ReservaHabitacionRepository(ReservationDbContext context) => _context = context;

    public async Task<ReservaHabitacionEntity?> ObtenerPorGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default)
        => await _context.ReservasHabitaciones.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ReservaHabitacionGuid == reservaHabitacionGuid, ct);

    public async Task<ReservaHabitacionEntity?> ObtenerParaActualizarPorGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default)
        => await _context.ReservasHabitaciones
            .FirstOrDefaultAsync(x => x.ReservaHabitacionGuid == reservaHabitacionGuid, ct);

    public async Task<IReadOnlyList<ReservaHabitacionEntity>> ListarPorReservaAsync(int idReserva, CancellationToken ct = default)
        => await _context.ReservasHabitaciones.AsNoTracking()
            .Where(x => x.IdReserva == idReserva)
            .OrderBy(x => x.FechaInicio)
            .ToListAsync(ct);

    public async Task AgregarRangoAsync(IEnumerable<ReservaHabitacionEntity> entities, CancellationToken ct = default)
        => await _context.ReservasHabitaciones.AddRangeAsync(entities, ct);

    public async Task AgregarAsync(ReservaHabitacionEntity entity, CancellationToken ct = default)
        => await _context.ReservasHabitaciones.AddAsync(entity, ct);

    public void Actualizar(ReservaHabitacionEntity entity) => _context.ReservasHabitaciones.Update(entity);

    public void Eliminar(ReservaHabitacionEntity entity) => _context.ReservasHabitaciones.Remove(entity);
}
