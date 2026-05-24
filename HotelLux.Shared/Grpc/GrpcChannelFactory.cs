using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.Configuration;

namespace HotelLux.Shared.Grpc;

public static class GrpcChannelFactory
{
    public static GrpcChannel Create(string address)
    {
        var normalized = NormalizeAddress(address);

        if (ShouldUseGrpcWeb())
        {
            var grpcWebHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
            return GrpcChannel.ForAddress(normalized, new GrpcChannelOptions { HttpHandler = grpcWebHandler });
        }

        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var inner = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan
        };

        return GrpcChannel.ForAddress(normalized, new GrpcChannelOptions { HttpHandler = inner });
    }

    /// <summary>
    /// Resuelve la URL gRPC: appsettings, variable de entorno homónima (AuditService__GrpcAddress) o localhost en dev.
    /// En Render use el puerto PORT del servicio destino, p. ej. http://nombre-servicio-audit:10000
    /// </summary>
    public static string ResolveAddress(
        IConfiguration configuration,
        string configurationKey,
        string? environmentVariableName,
        int localDevelopmentPort)
    {
        var fromConfig = configuration[configurationKey];
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return fromConfig.Trim();

        if (!string.IsNullOrWhiteSpace(environmentVariableName))
        {
            var fromEnv = Environment.GetEnvironmentVariable(environmentVariableName);
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv.Trim();
        }

        return $"http://localhost:{localDevelopmentPort}";
    }

    private static bool ShouldUseGrpcWeb()
    {
        var flag = Environment.GetEnvironmentVariable("GRPC_USE_WEB");
        if (string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(flag, "false", StringComparison.OrdinalIgnoreCase))
            return false;

        // Render y otros reverse proxies suelen hablar HTTP/1.1 hacia el contenedor.
        return string.Equals(
            Environment.GetEnvironmentVariable("RENDER"),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeAddress(string address)
    {
        if (!address.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !address.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return $"http://{address}";
        }

        return address;
    }
}
