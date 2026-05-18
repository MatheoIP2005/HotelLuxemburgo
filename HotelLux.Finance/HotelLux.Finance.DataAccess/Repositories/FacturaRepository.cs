using HotelLux.Finance.DataAccess.Context;
using HotelLux.Finance.DataAccess.Entities;
using HotelLux.Finance.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Finance.DataAccess.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly FinanceDbContext _db;
    public FacturaRepository(FinanceDbContext db) => _db = db;

    public async Task<FacturaEntity?> ObtenerPorGuidAsync(Guid facturaGuid, CancellationToken ct = default)
        => await _db.Facturas.AsNoTracking()
            .Include(x => x.Detalles)
            .Include(x => x.Pagos)
            .FirstOrDefaultAsync(x => x.FacturaGuid == facturaGuid && !x.EsEliminado, ct);

    public async Task<FacturaEntity?> ObtenerParaActualizarPorGuidAsync(Guid facturaGuid, CancellationToken ct = default)
        => await _db.Facturas
            .FirstOrDefaultAsync(x => x.FacturaGuid == facturaGuid && !x.EsEliminado, ct);

    public async Task<FacturaEntity?> ObtenerParaActualizarPorIdAsync(int idFactura, CancellationToken ct = default)
        => await _db.Facturas
            .FirstOrDefaultAsync(x => x.IdFactura == idFactura && !x.EsEliminado, ct);

    public async Task<IReadOnlyList<FacturaEntity>> ListarAsync(
        Guid? clienteGuid, Guid? sucursalGuid, string? estado, CancellationToken ct = default)
    {
        var q = _db.Facturas.AsNoTracking().Where(x => !x.EsEliminado);
        if (clienteGuid.HasValue) q = q.Where(x => x.ClienteGuid == clienteGuid.Value);
        if (sucursalGuid.HasValue) q = q.Where(x => x.SucursalGuid == sucursalGuid.Value);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => x.Estado == estado);
        return await q.OrderByDescending(x => x.FechaEmision).Take(200).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FacturaEntity>> ListarPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default)
        => await _db.Facturas.AsNoTracking()
            .Where(x => x.ReservaGuid == reservaGuid && !x.EsEliminado)
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync(ct);

    public async Task<int> ContarPorTipoAnioAsync(string tipoFactura, int anio, CancellationToken ct = default)
        => await _db.Facturas.AsNoTracking()
            .CountAsync(x => x.TipoFactura == tipoFactura && x.FechaEmision.Year == anio, ct);

    public async Task AgregarAsync(FacturaEntity entity, CancellationToken ct = default)
        => await _db.Facturas.AddAsync(entity, ct);

    public void Actualizar(FacturaEntity entity) => _db.Facturas.Update(entity);
}
