using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/images")]
[Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
public class InternalImagesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public InternalImagesController(IWebHostEnvironment env) => _env = env;

    /// <summary>Sube un archivo binario y devuelve la URL pública bajo /files/…</summary>
    [HttpPost("upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_485_760)]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiErrorResponse.Fail(400, "Archivo vacío o no enviado."));

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || ext.Length > 12)
            ext = ".bin";
        ext = ext.ToLowerInvariant();
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, safeName);
        await using (var fs = System.IO.File.Create(fullPath))
            await file.CopyToAsync(fs, ct);

        var url = $"/files/{safeName}";
        return Ok(ApiResponse<object>.Ok(new { url, fileName = safeName }, "Archivo cargado."));
    }
}
