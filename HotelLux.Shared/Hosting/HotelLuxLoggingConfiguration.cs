using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace HotelLux.Shared.Hosting;

public static class HotelLuxLoggingConfiguration
{
    public static WebApplicationBuilder ConfigureHotelLuxLogging(this WebApplicationBuilder builder)
    {
        if (OperatingSystem.IsWindows())
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }

        return builder;
    }
}
