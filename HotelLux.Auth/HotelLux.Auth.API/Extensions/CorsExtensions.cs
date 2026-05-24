namespace HotelLux.Auth.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCustomCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders(
                            "Grpc-Status",
                            "Grpc-Message",
                            "Grpc-Encoding",
                            "Grpc-Accept-Encoding");
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }
}
