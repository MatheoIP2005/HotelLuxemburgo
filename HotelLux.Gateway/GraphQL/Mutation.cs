using HotelLux.Gateway.GraphQL.Clients;
using HotelLux.Gateway.GraphQL.Types;

namespace HotelLux.Gateway.GraphQL;

public class Mutation
{
    public async Task<ReservationResult?> CreateReservation(
        CreateReservationInput input,
        [Service] ReservationGatewayClient client,
        CancellationToken cancellationToken) =>
        await client.CreateReservationAsync(input, cancellationToken);
}
