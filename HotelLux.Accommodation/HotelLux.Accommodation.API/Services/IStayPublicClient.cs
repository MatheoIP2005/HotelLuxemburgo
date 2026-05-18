namespace HotelLux.Accommodation.API.Services;

public interface IStayPublicClient
{
    Task<StayReviewsResult?> GetReviewsBySucursalAsync(Guid sucursalGuid, int page, int pageSize, CancellationToken ct);

    Task<StayRatingSummary?> GetRatingSummaryAsync(Guid sucursalGuid, CancellationToken ct);
}

public sealed class StayRatingSummary
{
    public bool TieneResenas { get; init; }
    public double PromedioGeneral { get; init; }
    public double PromedioLimpieza { get; init; }
    public double PromedioConfort { get; init; }
    public double PromedioUbicacion { get; init; }
    public double PromedioInstalaciones { get; init; }
    public double PromedioPersonal { get; init; }
    public double PromedioCalidadPrecio { get; init; }
    public int TotalResenas { get; init; }
}

public sealed class StayReviewsResult
{
    public IReadOnlyList<StayReviewDto> Items { get; init; } = Array.Empty<StayReviewDto>();
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

public sealed class StayReviewDto
{
    public Guid ValoracionGuid { get; init; }
    public Guid ClienteGuid { get; init; }
    public decimal PuntuacionGeneral { get; init; }
    public string ComentarioPositivo { get; init; } = "";
    public string ComentarioNegativo { get; init; } = "";
    public string TipoViaje { get; init; } = "";
    public string FechaPublicacion { get; init; } = "";
    public string RespuestaHotel { get; init; } = "";
    public string? NombreVisibleCliente { get; init; }
}
