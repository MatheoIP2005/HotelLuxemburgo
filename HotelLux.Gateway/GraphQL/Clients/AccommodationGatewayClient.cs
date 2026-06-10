using HotelLux.Gateway.GraphQL.Types;

namespace HotelLux.Gateway.GraphQL.Clients;

public class AccommodationGatewayClient : GatewayHttpClientBase
{
    public AccommodationGatewayClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public Task<AccommodationSearchPagedResult?> SearchAsync(
        string? destino,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int? numAdultos,
        int? numNinos,
        int? numHabitaciones,
        int pagina,
        int limite,
        CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(destino))
            query.Add($"destino={Uri.EscapeDataString(destino)}");
        if (fechaInicio.HasValue)
            query.Add($"fechaInicio={Uri.EscapeDataString(fechaInicio.Value.ToString("o"))}");
        if (fechaFin.HasValue)
            query.Add($"fechaFin={Uri.EscapeDataString(fechaFin.Value.ToString("o"))}");
        if (numAdultos.HasValue)
            query.Add($"num_adultos={numAdultos.Value}");
        if (numNinos.HasValue)
            query.Add($"num_ninos={numNinos.Value}");
        if (numHabitaciones.HasValue)
            query.Add($"num_habitaciones={numHabitaciones.Value}");
        query.Add($"pagina={pagina}");
        query.Add($"limite={limite}");

        var url = "api/v1/accommodations/search?" + string.Join("&", query);
        return GetAsync<AccommodationSearchPagedResult>(url, cancellationToken);
    }

    public Task<AccommodationDetailResult?> GetByIdAsync(
        Guid sucursalGuid,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (fechaInicio.HasValue)
            query.Add($"fechaInicio={Uri.EscapeDataString(fechaInicio.Value.ToString("o"))}");
        if (fechaFin.HasValue)
            query.Add($"fechaFin={Uri.EscapeDataString(fechaFin.Value.ToString("o"))}");

        var url = $"api/v1/accommodations/{sucursalGuid}";
        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        return GetAsync<AccommodationDetailResult>(url, cancellationToken);
    }

    public Task<AccommodationReviewsPagedResult?> GetReviewsAsync(
        Guid sucursalGuid,
        int pagina,
        int limite,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/accommodations/{sucursalGuid}/reviews?pagina={pagina}&limite={limite}";
        return GetAsync<AccommodationReviewsPagedResult>(url, cancellationToken);
    }
}
