using HotelLux.Gateway.GraphQL.Clients;
using HotelLux.Gateway.GraphQL.Types;

namespace HotelLux.Gateway.GraphQL;

public class Query
{
    public async Task<AccommodationSearchPagedResult?> AccommodationsSearch(
        string? destino,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int? numAdultos,
        int? numNinos,
        int? numHabitaciones,
        int pagina,
        int limite,
        [Service] AccommodationGatewayClient client,
        CancellationToken cancellationToken) =>
        await client.SearchAsync(
            destino, fechaInicio, fechaFin, numAdultos, numNinos, numHabitaciones,
            pagina, limite, cancellationToken);

    public async Task<AccommodationDetailResult?> Accommodation(
        Guid sucursalGuid,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        [Service] AccommodationGatewayClient client,
        CancellationToken cancellationToken) =>
        await client.GetByIdAsync(sucursalGuid, fechaInicio, fechaFin, cancellationToken);

    public async Task<AccommodationReviewsPagedResult?> AccommodationReviews(
        Guid sucursalGuid,
        int pagina,
        int limite,
        [Service] AccommodationGatewayClient client,
        CancellationToken cancellationToken) =>
        await client.GetReviewsAsync(sucursalGuid, pagina, limite, cancellationToken);

    public async Task<ReservationResult?> Reservation(
        Guid reservaGuid,
        [Service] ReservationGatewayClient client,
        CancellationToken cancellationToken) =>
        await client.GetReservationAsync(reservaGuid, cancellationToken);
}
