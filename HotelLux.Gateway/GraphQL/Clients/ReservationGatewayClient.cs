using HotelLux.Gateway.GraphQL.Types;

namespace HotelLux.Gateway.GraphQL.Clients;

public class ReservationGatewayClient : GatewayHttpClientBase
{
    public ReservationGatewayClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public Task<ReservationResult?> GetReservationAsync(Guid reservaGuid, CancellationToken cancellationToken) =>
        GetAsync<ReservationResult>($"api/v1/public/reservas/{reservaGuid}", cancellationToken);

    public Task<ReservationResult?> CreateReservationAsync(
        CreateReservationInput input,
        CancellationToken cancellationToken) =>
        PostAsync<CreateReservationInput, ReservationResult>(
            "api/v1/accommodations/reservas",
            input,
            cancellationToken);
}
