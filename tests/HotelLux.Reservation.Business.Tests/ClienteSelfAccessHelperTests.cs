using System.Security.Claims;
using HotelLux.Reservation.Business;
using Xunit;

namespace HotelLux.Reservation.Business.Tests;

public class ClienteSelfAccessHelperTests
{
    [Fact]
    public void Admin_es_staff_y_puede_ver_cualquier_cliente()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "test"));
        Assert.True(ClienteSelfAccessHelper.EsStaff(user));
        Assert.True(ClienteSelfAccessHelper.PuedeVerCliente(user, Guid.NewGuid()));
    }

    [Fact]
    public void Vendedor_es_staff_y_puede_ver_cualquier_cliente()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "VENDEDOR")
        }, "test"));
        Assert.True(ClienteSelfAccessHelper.EsStaff(user));
        Assert.True(ClienteSelfAccessHelper.PuedeVerCliente(user, Guid.NewGuid()));
    }

    [Fact]
    public void Usuario_sin_rol_staff_no_puede_ver_clientes()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "DESCONOCIDO")
        }, "test"));
        Assert.False(ClienteSelfAccessHelper.EsStaff(user));
        Assert.False(ClienteSelfAccessHelper.PuedeVerCliente(user, Guid.NewGuid()));
    }
}
