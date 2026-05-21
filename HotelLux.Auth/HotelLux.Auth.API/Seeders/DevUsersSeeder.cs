using HotelLux.Auth.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Auth.API.Seeders;

/// <summary>
/// Garantiza credenciales, roles y estado ACT de usuarios de desarrollo (admin / vendedor).
/// </summary>
public static class DevUsersSeeder
{
    private static readonly (string Username, string Password, string Rol)[] DevUsers =
    [
        ("admin", "admin1234", "ADMIN"),
        ("vendedor", "vendedor1234", "VENDEDOR")
    ];

    public static async Task EnsureDevCredentialsAsync(AuthDbContext db, CancellationToken ct)
    {
        var changed = false;

        foreach (var (username, password, rolNombre) in DevUsers)
        {
            var u = await db.UsuarioApps
                .Include(x => x.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(x => x.Username == username, ct);

            if (u is null || !string.Equals(u.CreadoPorUsuario, "system", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!u.Activo || u.EstadoUsuario != "ACT" || u.EsEliminado)
            {
                u.Activo = true;
                u.EstadoUsuario = "ACT";
                u.EsEliminado = false;
                u.MotivoInhabilitacion = null;
                u.FechaInhabilitacionUtc = null;
                changed = true;
            }

            var hashOk = u.PasswordHash.StartsWith("$2", StringComparison.Ordinal)
                && BCrypt.Net.BCrypt.Verify(password, u.PasswordHash);

            if (!hashOk)
            {
                u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 11);
                u.PasswordSalt = string.Empty;
                changed = true;
            }

            var rol = await db.Roles.FirstOrDefaultAsync(r => r.NombreRol == rolNombre, ct);
            if (rol is not null)
            {
                if (!rol.Activo || rol.EsEliminado || rol.EstadoRol != "ACT")
                {
                    rol.Activo = true;
                    rol.EsEliminado = false;
                    rol.EstadoRol = "ACT";
                    changed = true;
                }

                var link = u.UsuarioRoles.FirstOrDefault(ur => ur.IdRol == rol.IdRol);
                if (link is null)
                {
                    db.UsuariosRoles.Add(new DataAccess.Entities.UsuarioRolEntity
                    {
                        IdUsuario = u.IdUsuario,
                        IdRol = rol.IdRol,
                        EstadoUsuarioRol = "ACT",
                        Activo = true,
                        EsEliminado = false,
                        CreadoPorUsuario = "system",
                        FechaRegistroUtc = DateTimeOffset.UtcNow
                    });
                    changed = true;
                }
                else if (!link.Activo || link.EsEliminado || link.EstadoUsuarioRol != "ACT")
                {
                    link.Activo = true;
                    link.EsEliminado = false;
                    link.EstadoUsuarioRol = "ACT";
                    changed = true;
                }
            }
        }

        if (changed)
            await db.SaveChangesAsync(ct);
    }
}
