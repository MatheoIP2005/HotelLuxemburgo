using HotelLux.Finance.DataAccess.Context;
using HotelLux.Finance.DataAccess.Entities;
using HotelLux.Finance.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Finance.DataAccess.Repositories;

public class PagoRepository : IPagoRepository
{
    private readonly FinanceDbContext _db;
    public PagoRepository(FinanceDbContext db) => _db = db;

    public async Task<PagoEntity?> ObtenerPorGuidAsync(Guid pagoGuid, CancellationToken ct = default)
        => await _db.Pagos.AsNoTracking()
            .Include(x => x.Factura)
            .FirstOrDefaultAsync(x => x.PagoGuid == pagoGuid, ct);

    public async Task<PagoEntity?> ObtenerParaActualizarAsync(Guid pagoGuid, CancellationToken ct = default)
        => await _db.Pagos.FirstOrDefaultAsync(x => x.PagoGuid == pagoGuid, ct);

    public async Task<IReadOnlyList<PagoEntity>> ListarPorFacturaGuidAsync(Guid facturaGuid, CancellationToken ct = default)
    {
        var idFactura = await _db.Facturas.AsNoTracking()
            .Where(f => f.FacturaGuid == facturaGuid && !f.EsEliminado)
            .Select(f => f.IdFactura)
            .FirstOrDefaultAsync(ct);
        if (idFactura == 0) return Array.Empty<PagoEntity>();
        return await _db.Pagos.AsNoTracking()
            .Where(x => x.IdFactura == idFactura)
            .OrderByDescending(x => x.FechaRegistroUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PagoEntity>> ListarFiltradoAsync(
        Guid? facturaGuid,
        Guid? reservaGuid,
        string? estadoPago,
        string? metodoPago,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta,
        int maxResults,
        CancellationToken ct = default)
    {
        maxResults = Math.Clamp(maxResults, 1, 500);
        var q = _db.Pagos.AsNoTracking().Include(x => x.Factura).AsQueryable();

        if (facturaGuid.HasValue)
        {
            var idF = await _db.Facturas.AsNoTracking()
                .Where(f => f.FacturaGuid == facturaGuid && !f.EsEliminado)
                .Select(f => f.IdFactura)
                .FirstOrDefaultAsync(ct);
            if (idF == 0) return Array.Empty<PagoEntity>();
            q = q.Where(p => p.IdFactura == idF);
        }

        if (reservaGuid.HasValue)
            q = q.Where(p => p.ReservaGuid == reservaGuid.Value);

        if (!string.IsNullOrWhiteSpace(estadoPago))
        {
            var e = estadoPago.Trim().ToUpperInvariant();
            q = q.Where(p => p.EstadoPago == e);
        }

        if (!string.IsNullOrWhiteSpace(metodoPago))
        {
            var m = metodoPago.Trim();
            q = q.Where(p => p.MetodoPago == m);
        }

        if (fechaDesde.HasValue)
            q = q.Where(p => p.FechaPagoUtc >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            q = q.Where(p => p.FechaPagoUtc <= fechaHasta.Value);

        return await q.OrderByDescending(x => x.FechaPagoUtc).Take(maxResults).ToListAsync(ct);
    }

    public async Task AgregarAsync(PagoEntity entity, CancellationToken ct = default)
        => await _db.Pagos.AddAsync(entity, ct);

    public void Actualizar(PagoEntity entity) => _db.Pagos.Update(entity);
}
