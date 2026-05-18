using HotelLux.Auth.DataAccess.Context;
using HotelLux.Auth.DataAccess.Entities;
using HotelLux.Auth.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Auth.DataAccess.Repositories;

public class UsuarioAppRepository : IUsuarioAppRepository
{
    private readonly AuthDbContext _context;

    public UsuarioAppRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioAppEntity?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioApps
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario && !x.EsEliminado, cancellationToken);
    }

    public async Task<UsuarioAppEntity?> ObtenerPorGuidAsync(Guid usuarioGuid, CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioApps
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
                .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(x => x.UsuarioGuid == usuarioGuid && !x.EsEliminado, cancellationToken);
    }

    public async Task<UsuarioAppEntity?> ObtenerParaActualizarAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioApps
            .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario && !x.EsEliminado, cancellationToken);
    }

    public async Task<UsuarioAppEntity?> ObtenerPorUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioApps
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
                .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Username == username && !x.EsEliminado && x.Activo, cancellationToken);
    }

    public async Task<UsuarioAppEntity?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioApps
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Correo == correo && !x.EsEliminado, cancellationToken);
    }

    public async Task<IReadOnlyList<UsuarioAppEntity>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioApps
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
                .ThenInclude(x => x.Rol)
            .Where(x => !x.EsEliminado)
            .OrderBy(x => x.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(UsuarioAppEntity usuario, CancellationToken cancellationToken = default)
    {
        await _context.UsuarioApps.AddAsync(usuario, cancellationToken);
    }

    public void Actualizar(UsuarioAppEntity usuario)
    {
        _context.UsuarioApps.Update(usuario);
    }

    public void EliminarLogico(UsuarioAppEntity usuario)
    {
        usuario.EsEliminado = true;
        usuario.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        _context.UsuarioApps.Update(usuario);
    }

    public async Task<UsuarioRolEntity?> ObtenerUsuarioRolPorUsuarioYRolAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosRoles
            .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario && x.IdRol == idRol, cancellationToken);
    }

    public async Task AgregarUsuarioRolAsync(UsuarioRolEntity usuarioRol, CancellationToken cancellationToken = default)
    {
        await _context.UsuariosRoles.AddAsync(usuarioRol, cancellationToken);
    }

    public void ActualizarUsuarioRol(UsuarioRolEntity usuarioRol)
    {
        _context.UsuariosRoles.Update(usuarioRol);
    }
}
