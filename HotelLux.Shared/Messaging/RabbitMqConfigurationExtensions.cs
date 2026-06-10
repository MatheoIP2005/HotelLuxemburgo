using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelLux.Shared.Messaging;

public static class RabbitMqConfigurationExtensions
{
    public const string CloudAmqpUrlEnvironmentVariable = "CLOUDAMQP_URL";

    public static RabbitMqSettings GetRabbitMqSettings(this IConfiguration configuration)
    {
        var settings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        if (string.IsNullOrWhiteSpace(settings.Uri))
        {
            var cloudAmqpUrl = configuration[CloudAmqpUrlEnvironmentVariable]
                ?? Environment.GetEnvironmentVariable(CloudAmqpUrlEnvironmentVariable);

            if (!string.IsNullOrWhiteSpace(cloudAmqpUrl))
                settings.Uri = cloudAmqpUrl;
        }

        return settings;
    }

    public static void ConfigureRabbitMqHost(
        this IRabbitMqBusFactoryConfigurator cfg,
        RabbitMqSettings settings)
    {
        var connection = RabbitMqConnectionResolver.Resolve(settings);

        cfg.Host(connection.Host, connection.Port, connection.VirtualHost, h =>
        {
            h.Username(connection.Username);
            h.Password(connection.Password);

            if (connection.UseSsl)
            {
                h.UseSsl(ssl =>
                {
                    ssl.Protocol = SslProtocols.Tls12 | SslProtocols.Tls13;
                });
            }
        });
    }

    public static IServiceCollection AddHotelLuxRabbitMqPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetRabbitMqSettings();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((_, cfg) => cfg.ConfigureRabbitMqHost(settings));
        });

        return services;
    }
}
