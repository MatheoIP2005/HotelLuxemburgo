using HotelLux.Reservation.DataManagement.Interfaces;
using HotelLux.Reservation.DataManagement.Mappers;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Services;

public class ReservaDataService : IReservaDataService
{
    private readonly IUnitOfWork _uow;
    public ReservaDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<ReservaDataModel?> ObtenerPorGuidAsync(Guid reservaGuid, CancellationToken ct = default)
    {
        var e = await _uow.ReservaRepository.ObtenerPorGuidAsync(reservaGuid, ct);
        if (e is null) return null;
        var m = ReservaDataMapper.ToDataModel(e);
        if (m.ClienteGuid == Guid.Empty && e.IdCliente > 0)
        {
            var c = await _uow.ClienteRepository.ObtenerPorIdAsync(e.IdCliente, ct);
            if (c is not null) m.ClienteGuid = c.ClienteGuid;
        }

        return m;
    }

    public async Task<ReservaDataModel?> ObtenerPorCodigoAsync(string codigoReserva, CancellationToken ct = default)
    {
        var e = await _uow.ReservaRepository.ObtenerPorCodigoAsync(codigoReserva, ct);
        return e is null ? null : ReservaDataMapper.ToDataModel(e);
    }

    public async Task<IReadOnlyList<ReservaDataModel>> ListarAsync(CancellationToken ct = default)
    {
        var list = await _uow.ReservaRepository.ListarAsync(ct);
        return list.Select(ReservaDataMapper.ToDataModel).ToList();
    }

    public async Task<PagedDataResult<ReservaDataModel>> BuscarAsync(
        Guid? clienteGuid, Guid? sucursalGuid, string? estadoReserva,
        DateOnly? fechaDesde, DateOnly? fechaHasta, string? origenCanal,
        int pagina, int limite, CancellationToken ct = default)
    {
        var (items, total) = await _uow.ReservaRepository.BuscarAsync(
            clienteGuid, sucursalGuid, estadoReserva,
            fechaDesde, fechaHasta, origenCanal,
            pagina, limite, ct);

        return new PagedDataResult<ReservaDataModel>
        {
            Items = items.Select(ReservaDataMapper.ToDataModel).ToList(),
            Total = total,
            Pagina = pagina,
            Limite = limite
        };
    }

    public async Task<ReservaDataModel> CrearAsync(ReservaDataModel model, CancellationToken ct = default)
    {
        var entity = ReservaDataMapper.ToEntity(model);
        var cliente = await _uow.ClienteRepository.ObtenerPorGuidAsync(model.ClienteGuid, ct)
            ?? throw new InvalidOperationException($"Cliente '{model.ClienteGuid}' no existe.");
        entity.IdCliente = cliente.IdCliente;

        if (entity.ReservaGuid == Guid.Empty)
            entity.ReservaGuid = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(entity.CodigoReserva))
            entity.CodigoReserva =
                $"RES-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

        entity.FechaRegistroUtc = DateTimeOffset.UtcNow;
        entity.EstadoReserva = "PEN";
        entity.ServicioOrigen = "reservation-service";

        foreach (var hab in model.Habitaciones)
        {
            var habEntity = ReservaHabitacionDataMapper.ToEntity(hab);
            habEntity.ReservaHabitacionGuid = Guid.NewGuid();
            habEntity.FechaRegistroUtc = DateTimeOffset.UtcNow;
            habEntity.EstadoDetalle = "PEN";
            habEntity.ServicioOrigen = "reservation-service";
            entity.ReservasHabitaciones.Add(habEntity);
        }

        await _uow.ReservaRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        var reloaded = await _uow.ReservaRepository.ObtenerPorIdAsync(entity.IdReserva, ct) ?? entity;
        return ReservaDataMapper.ToDataModel(reloaded);
    }

    public async Task<ReservaDataModel?> ActualizarAsync(ReservaDataModel model, CancellationToken ct = default)
    {
        var entity = await _uow.ReservaRepository.ObtenerParaActualizarAsync(model.IdReserva, ct);
        if (entity is null) return null;

        entity.EstadoReserva = model.EstadoReserva;
        entity.SubtotalReserva = model.SubtotalReserva;
        entity.ValorIva = model.ValorIva;
        entity.TotalReserva = model.TotalReserva;
        entity.DescuentoAplicado = model.DescuentoAplicado;
        entity.SaldoPendiente = model.SaldoPendiente;
        entity.FechaConfirmacionUtc = model.FechaConfirmacionUtc;
        entity.FechaCancelacionUtc = model.FechaCancelacionUtc;
        entity.MotivoCancelacion = model.MotivoCancelacion;
        entity.Observaciones = model.Observaciones;
        entity.ModificadoPorUsuario = model.ModificadoPorUsuario;
        entity.FechaModificacionUtc = model.FechaModificacionUtc;
        entity.ModificacionIp = model.ModificacionIp;

        _uow.ReservaRepository.Actualizar(entity);
        await _uow.SaveChangesAsync(ct);

        var reloaded = await _uow.ReservaRepository.ObtenerPorIdAsync(entity.IdReserva, ct);
        return reloaded is null ? null : ReservaDataMapper.ToDataModel(reloaded);
    }

    public async Task<bool> EliminarLogicoAsync(Guid reservaGuid, string usuario, CancellationToken ct = default)
    {
        var entity = await _uow.ReservaRepository.ObtenerParaActualizarPorGuidAsync(reservaGuid, ct);
        if (entity is null) return false;

        entity.EsEliminado = true;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        entity.MotivoInhabilitacion = $"Eliminado por {usuario}";
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;

        _uow.ReservaRepository.Actualizar(entity);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task RecalcularTotalesDesdeHabitacionesAsync(
        Guid reservaGuid, string usuario, CancellationToken ct = default)
    {
        var r = await _uow.ReservaRepository.ObtenerParaActualizarPorGuidAsync(reservaGuid, ct)
            ?? throw new InvalidOperationException($"Reserva '{reservaGuid}' no encontrada.");

        var lines = r.ReservasHabitaciones
            .Where(h => h.EstadoDetalle != "CAN")
            .ToList();

        var totalAnt = r.TotalReserva;
        var saldoAnt = r.SaldoPendiente;
        var pagado = Math.Max(0, totalAnt - saldoAnt);

        if (lines.Count == 0)
        {
            r.SubtotalReserva = 0;
            r.ValorIva = 0;
            r.TotalReserva = 0;
            r.SaldoPendiente = 0;
        }
        else
        {
            r.SubtotalReserva = lines.Sum(h => h.SubtotalLinea);
            r.ValorIva = lines.Sum(h => h.ValorIvaLinea);
            r.TotalReserva = lines.Sum(h => h.TotalLinea);
            r.FechaInicio = lines.Min(h => h.FechaInicio);
            r.FechaFin = lines.Max(h => h.FechaFin);
            r.SaldoPendiente = Math.Max(0, r.TotalReserva - pagado);
        }

        r.ModificadoPorUsuario = usuario;
        r.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.ReservaRepository.Actualizar(r);
        await _uow.SaveChangesAsync(ct);
    }
}
