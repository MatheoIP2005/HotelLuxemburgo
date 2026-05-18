using HotelLux.Auth.Business.Interfaces;

namespace HotelLux.Auth.Business.Services;

public class PermisoService : IPermisoService
{
    // Los permisos no están implementados aún.
    // Retorna lista vacía para no bloquear arranque ni DI.
    public Task<IReadOnlyList<string>> ObtenerPermisosAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
}
