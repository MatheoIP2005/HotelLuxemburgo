using HotelLux.Auth.DataAccess.Context;
using HotelLux.Auth.DataAccess.Entities;
using HotelLux.Auth.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Auth.DataAccess.Repositories;

public class RolRepository : IRolRepository
{
    private readonly AuthDbContext _context;

    public RolRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<RolEntity?> ObtenerPorIdAsync(int idRol, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdRol == idRol && !x.EsEliminado, cancellationToken);
    }

    public async Task<RolEntity?> ObtenerPorGuidAsync(Guid rolGuid, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RolGuid == rolGuid && !x.EsEliminado, cancellationToken);
    }

    public async Task<RolEntity?> ObtenerParaActualizarAsync(int idRol, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(x => x.IdRol == idRol && !x.EsEliminado, cancellationToken);
    }

    public async Task<RolEntity?> ObtenerPorNombreAsync(string nombreRol, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NombreRol == nombreRol && !x.EsEliminado, cancellationToken);
    }

    public async Task<IReadOnlyList<RolEntity>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(x => !x.EsEliminado)
            .OrderBy(x => x.NombreRol)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(RolEntity rol, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(rol, cancellationToken);
    }

    public void Actualizar(RolEntity rol)
    {
        _context.Roles.Update(rol);
    }

    public void EliminarLogico(RolEntity rol)
    {
        rol.EsEliminado = true;
        rol.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        _context.Roles.Update(rol);
    }
}
