namespace HotelLux.Gateway.GraphQL;

public static class GatewayServiceRegistration
{
    public static IServiceCollection AddGatewayGraphQl(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var accommodationBase = configuration["ReverseProxy:Clusters:accommodation:Destinations:api:Address"]
            ?? "http://127.0.0.1:5002/";
        var reservationBase = configuration["ReverseProxy:Clusters:reservation:Destinations:api:Address"]
            ?? "http://127.0.0.1:5003/";

        services.AddHttpContextAccessor();

        services.AddHttpClient<Clients.AccommodationGatewayClient>(client =>
            client.BaseAddress = new Uri(accommodationBase));

        services.AddHttpClient<Clients.ReservationGatewayClient>(client =>
            client.BaseAddress = new Uri(reservationBase));

        services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>();

        return services;
    }
}
