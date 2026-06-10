namespace HotelLux.Shared.Messaging;

public sealed class RabbitMqResolvedConnection
{
    public required string Host { get; init; }
    public required ushort Port { get; init; }
    public required string VirtualHost { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required bool UseSsl { get; init; }
}

public static class RabbitMqConnectionResolver
{
    public static RabbitMqResolvedConnection Resolve(RabbitMqSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!string.IsNullOrWhiteSpace(settings.Uri))
            return ResolveFromUri(settings.Uri, settings);

        var useSsl = settings.UseSsl;
        var port = settings.Port ?? (useSsl ? 5671 : 5672);

        return new RabbitMqResolvedConnection
        {
            Host = settings.Host,
            Port = (ushort)port,
            VirtualHost = NormalizeVirtualHost(settings.VirtualHost),
            Username = settings.Username,
            Password = settings.Password,
            UseSsl = useSsl
        };
    }

    internal static RabbitMqResolvedConnection ResolveFromUri(string uriValue, RabbitMqSettings settings)
    {
        if (!Uri.TryCreate(uriValue, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("RabbitMq:Uri no es una URL absoluta valida.");

        var scheme = uri.Scheme.ToLowerInvariant();
        var useSsl = scheme is "amqps" or "rabbitmqs";
        if (scheme is not ("amqp" or "amqps" or "rabbitmq" or "rabbitmqs"))
            throw new InvalidOperationException($"Esquema RabbitMQ no soportado: {uri.Scheme}.");

        var port = uri.IsDefaultPort
            ? (ushort?)(useSsl ? 5671 : 5672)
            : (ushort)uri.Port;

        var username = settings.Username;
        var password = settings.Password;
        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            username = Uri.UnescapeDataString(parts[0]);
            password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
        }

        return new RabbitMqResolvedConnection
        {
            Host = uri.Host,
            Port = port ?? (useSsl ? (ushort)5671 : (ushort)5672),
            VirtualHost = ParseVirtualHostFromUri(uri),
            Username = username,
            Password = password,
            UseSsl = useSsl
        };
    }

    internal static string ParseVirtualHostFromUri(Uri uri)
    {
        var path = uri.AbsolutePath;
        if (string.IsNullOrEmpty(path) || path == "/")
            return "/";

        return NormalizeVirtualHost(Uri.UnescapeDataString(path.TrimStart('/')));
    }

    internal static string NormalizeVirtualHost(string? virtualHost)
    {
        if (string.IsNullOrWhiteSpace(virtualHost) || virtualHost == "/")
            return "/";

        return virtualHost.TrimStart('/');
    }
}
