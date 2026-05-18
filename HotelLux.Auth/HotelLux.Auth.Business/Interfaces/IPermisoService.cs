namespace HotelLux.Auth.Business.Interfaces;

public interface IPermisoService
{
    Task<IReadOnlyList<string>> ObtenerPermisosAsync(CancellationToken cancellationToken = default);
}
