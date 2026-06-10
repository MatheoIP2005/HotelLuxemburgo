using HotelLux.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HotelLux.Shared.Tests;

public class RabbitMqConnectionResolverTests
{
    [Fact]
    public void Resolve_LocalDefaults_UsesPort5672WithoutSsl()
    {
        var settings = new RabbitMqSettings
        {
            Host = "localhost",
            VirtualHost = "/",
            Username = "guest",
            Password = "guest"
        };

        var connection = RabbitMqConnectionResolver.Resolve(settings);

        Assert.Equal("localhost", connection.Host);
        Assert.Equal((ushort)5672, connection.Port);
        Assert.Equal("/", connection.VirtualHost);
        Assert.False(connection.UseSsl);
    }

    [Fact]
    public void Resolve_UseSslWithoutPort_UsesPort5671()
    {
        var settings = new RabbitMqSettings
        {
            Host = "broker.example.com",
            UseSsl = true,
            VirtualHost = "prod",
            Username = "user",
            Password = "pass"
        };

        var connection = RabbitMqConnectionResolver.Resolve(settings);

        Assert.Equal((ushort)5671, connection.Port);
        Assert.True(connection.UseSsl);
        Assert.Equal("prod", connection.VirtualHost);
    }

    [Fact]
    public void ResolveFromUri_Amqp_ParsesHostPortVirtualHostAndCredentials()
    {
        var connection = RabbitMqConnectionResolver.Resolve(new RabbitMqSettings
        {
            Uri = "amqp://user:pass@example.com:5672/myvhost"
        });

        Assert.Equal("example.com", connection.Host);
        Assert.Equal((ushort)5672, connection.Port);
        Assert.Equal("myvhost", connection.VirtualHost);
        Assert.Equal("user", connection.Username);
        Assert.Equal("pass", connection.Password);
        Assert.False(connection.UseSsl);
    }

    [Fact]
    public void ResolveFromUri_Amqps_InfersSslAndDefaultPort5671()
    {
        var connection = RabbitMqConnectionResolver.Resolve(new RabbitMqSettings
        {
            Uri = "amqps://user:pass@example.com/myvhost"
        });

        Assert.Equal("example.com", connection.Host);
        Assert.Equal((ushort)5671, connection.Port);
        Assert.Equal("myvhost", connection.VirtualHost);
        Assert.True(connection.UseSsl);
    }

    [Fact]
    public void GetRabbitMqSettings_UsesCloudAmqpUrlWhenUriMissing()
    {
        var previous = Environment.GetEnvironmentVariable(RabbitMqConfigurationExtensions.CloudAmqpUrlEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(
                RabbitMqConfigurationExtensions.CloudAmqpUrlEnvironmentVariable,
                "amqps://cloud:secret@cloud.example.com:5671/cloudvhost");

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RabbitMq:Host"] = "localhost",
                    ["RabbitMq:AuditQueue"] = "hotellux.audit.events"
                })
                .Build();

            var settings = configuration.GetRabbitMqSettings();

            Assert.Equal("amqps://cloud:secret@cloud.example.com:5671/cloudvhost", settings.Uri);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                RabbitMqConfigurationExtensions.CloudAmqpUrlEnvironmentVariable,
                previous);
        }
    }

    [Fact]
    public void GetRabbitMqSettings_PrefersConfiguredUriOverCloudAmqpUrl()
    {
        var previous = Environment.GetEnvironmentVariable(RabbitMqConfigurationExtensions.CloudAmqpUrlEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(
                RabbitMqConfigurationExtensions.CloudAmqpUrlEnvironmentVariable,
                "amqp://ignored:ignored@ignored.example.com/ignored");

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RabbitMq:Uri"] = "amqp://user:pass@preferred.example.com:5672/preferred"
                })
                .Build();

            var settings = configuration.GetRabbitMqSettings();
            var connection = RabbitMqConnectionResolver.Resolve(settings);

            Assert.Equal("preferred.example.com", connection.Host);
            Assert.Equal("preferred", connection.VirtualHost);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                RabbitMqConfigurationExtensions.CloudAmqpUrlEnvironmentVariable,
                previous);
        }
    }
}
