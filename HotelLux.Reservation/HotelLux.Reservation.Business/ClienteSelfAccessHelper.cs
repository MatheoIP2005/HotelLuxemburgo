using System.Security.Claims;

namespace HotelLux.Reservation.Business;

/// <summary>Reglas de acceso para usuarios con rol CLIENTE vinculados a un cliente (claim cliente_guid).</summary>
public static class ClienteSelfAccessHelper
{
    public static Guid? TryGetClienteGuidClaim(ClaimsPrincipal user)
    {
        var v = user.FindFirst("cliente_guid")?.Value;
        return Guid.TryParse(v, out var g) ? g : null;
    }

    public static bool EsStaff(ClaimsPrincipal user) =>
        user.IsInRole("ADMINISTRADOR") || user.IsInRole("RECEPCIONISTA") || user.IsInRole("VENDEDOR");

    public static bool PuedeVerCliente(ClaimsPrincipal user, Guid clienteGuid)
    {
        if (EsStaff(user)) return true;
        if (!user.IsInRole("CLIENTE")) return false;
        var cg = TryGetClienteGuidClaim(user);
        return cg.HasValue && cg.Value == clienteGuid;
    }

    public static bool PuedeVerReservaDeCliente(ClaimsPrincipal user, Guid reservaClienteGuid) =>
        PuedeVerCliente(user, reservaClienteGuid);
}
