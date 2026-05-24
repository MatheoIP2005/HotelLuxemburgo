using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace HotelLux.Shared.Hosting;

/// <summary>
/// En Render/cloud solo se expone PORT: REST y gRPC comparten Http1AndHttp2 en ese puerto.
/// En desarrollo local se puede usar GRPC_PORT dedicado (solo HTTP/2) si difiere de PORT.
/// </summary>
public static class KestrelEndpointConfiguration
{
    public static IWebHostBuilder ConfigureHotelLuxKestrel(
        this IWebHostBuilder webHost,
        IHostEnvironment environment,
        int defaultHttpPort,
        int defaultGrpcPort)
    {
        // Evita que ASPNETCORE_URLS / --urls cree listeners HTTP/1 que rompen gRPC (HTTP_1_1_REQUIRED).
        webHost.PreferHostingUrls(false);

        webHost.ConfigureKestrel(options =>
        {
            var httpPort = ResolvePort("PORT", defaultHttpPort);
            int? devGrpcPort = null;
            if (environment.IsDevelopment())
            {
                var grpcPort = int.TryParse(Environment.GetEnvironmentVariable("GRPC_PORT"), out var fromEnv)
                    ? fromEnv
                    : defaultGrpcPort;
                if (grpcPort != httpPort)
                    devGrpcPort = grpcPort;
            }

            // REST + GrpcWeb (HTTP/1.1). h2c nativo no funciona en el mismo puerto cleartext (ver issue dotnet/aspnetcore#56984).
            options.ListenAnyIP(httpPort, listen =>
                listen.Protocols = HttpProtocols.Http1);

            if (devGrpcPort is int separateGrpcPort)
            {
                options.ListenAnyIP(separateGrpcPort, listen =>
                    listen.Protocols = HttpProtocols.Http2);
            }
        });

        return webHost;
    }

    private static int ResolvePort(string envName, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(envName), out var port) ? port : fallback;
}
