using Microsoft.OpenApi.Models;

namespace HotelLux.Gateway.Swagger;

public static class GatewaySwaggerExtensions
{
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("gateway", new OpenApiInfo
            {
                Title = "HotelLux — Portal de documentación",
                Version = "v1",
                Description = BuildIndexDescription()
            });

            options.DocumentFilter<GatewayIndexDocumentFilter>();
        });

        return services;
    }

    public static WebApplication UseGatewaySwaggerUi(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "HotelLux API — Vista global";
            options.RoutePrefix = "swagger";
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelsExpandDepth(1);

            options.SwaggerEndpoint("/swagger/gateway/swagger.json", "00 — Índice y guía");

            var order = 1;
            foreach (var doc in GatewaySwaggerCatalog.Services)
            {
                var prefix = $"{order:D2}";
                options.SwaggerEndpoint(
                    GatewaySwaggerCatalog.ProxyPath(doc.RouteKey, doc.SwaggerRelativePath),
                    $"{prefix} — {doc.DisplayName}");
                order++;
            }
        });

        return app;
    }

    private static string BuildIndexDescription()
    {
        var lines = new List<string>
        {
            "Documentación unificada del ecosistema HotelLux. Selecciona cada microservicio en el desplegable superior.",
            "",
            "**Contratos normativos (JSON esperado):**",
            "- `endpoints_publicas.txt` — 6 endpoints del marketplace (vía Gateway :5000)",
            "- `endpoints_locales.txt` — endpoints internos / administración",
            "",
            "**Probar APIs:** base `http://127.0.0.1:5000` (Try it out usa el Gateway).",
            "",
            "**Requisito:** todos los microservicios deben estar en ejecución (OpenAPI expuesto en cada puerto).",
            "",
            "| Microservicio | Contenido |",
            "|---|---|"
        };

        foreach (var doc in GatewaySwaggerCatalog.Services)
            lines.Add($"| {doc.DisplayName} | {doc.Hint} |");

        return string.Join("\n", lines);
    }
}

/// <summary>Documento índice sin operaciones HTTP (solo guía).</summary>
internal sealed class GatewayIndexDocumentFilter : Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext context)
    {
        if (!context.DocumentName.Equals("gateway", StringComparison.OrdinalIgnoreCase))
            return;

        swaggerDoc.Paths = new OpenApiPaths();
        swaggerDoc.Tags =
        [
            new OpenApiTag { Name = "Guía", Description = "Usa el selector superior para abrir cada microservicio." },
            new OpenApiTag { Name = "Públicas", Description = "Ver Accommodation y Reservation en el catálogo." },
            new OpenApiTag { Name = "Locales", Description = "Rutas /api/v1/internal/** en cada servicio." }
        ];
    }
}
