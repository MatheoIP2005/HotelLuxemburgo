using System.Security.Claims;
using HotelLux.Reservation.Business;
using Xunit;

namespace HotelLux.Reservation.Business.Tests;

public class ClienteSelfAccessHelperTests
{
    [Fact]
    public void Staff_puede_ver_cualquier_cliente()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "RECEPCIONISTA")
        }, "test"));
        Assert.True(ClienteSelfAccessHelper.PuedeVerCliente(user, Guid.NewGuid()));
    }

    [Fact]
    public void Cliente_solo_ve_su_guid()
    {
        var id = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "CLIENTE"),
            new Claim("cliente_guid", id.ToString())
        }, "test"));
        Assert.True(ClienteSelfAccessHelper.PuedeVerCliente(user, id));
        Assert.False(ClienteSelfAccessHelper.PuedeVerCliente(user, Guid.NewGuid()));
    }

    [Fact]
    public void Cliente_sin_claim_no_puede()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "CLIENTE")
        }, "test"));
        Assert.False(ClienteSelfAccessHelper.PuedeVerCliente(user, Guid.NewGuid()));
    }
}
