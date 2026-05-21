using System.Security.Claims;

namespace HotelLux.Reservation.Business;

/// <summary>
/// Reglas de acceso para staff sobre clientes y sus reservas.
/// En el modelo actual solo existen dos roles (ADMIN y VENDEDOR); ambos son staff con acceso pleno
/// a los recursos de cliente. El claim opcional <c>cliente_guid</c> se conserva por compatibilidad.
/// </summary>
public static class ClienteSelfAccessHelper
{
    public static Guid? TryGetClienteGuidClaim(ClaimsPrincipal user)
    {
        var v = user.FindFirst("cliente_guid")?.Value;
        return Guid.TryParse(v, out var g) ? g : null;
    }

    public static bool EsStaff(ClaimsPrincipal user) =>
        user.IsInRole("ADMIN") || user.IsInRole("VENDEDOR");

    public static bool PuedeVerCliente(ClaimsPrincipal user, Guid clienteGuid) =>
        EsStaff(user);

    public static bool PuedeVerReservaDeCliente(ClaimsPrincipal user, Guid reservaClienteGuid) =>
        PuedeVerCliente(user, reservaClienteGuid);
}
