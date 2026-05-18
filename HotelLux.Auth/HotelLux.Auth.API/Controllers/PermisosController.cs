using Asp.Versioning;
using HotelLux.Auth.API.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Auth.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/permisos")]
[Authorize]
public class PermisosController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public IActionResult Listar()
    {
        IReadOnlyList<string> vacio = Array.Empty<string>();
        return Ok(ApiResponse<IReadOnlyList<string>>.Ok(vacio, "Sin permisos configurados (stub)."));
    }
}
