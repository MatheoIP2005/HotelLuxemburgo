namespace HotelLux.Gateway.Swagger;

public static class GatewaySwaggerExtensions
{
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient();
        services.AddSingleton<GatewayEndpointSpecCatalog>();
        services.AddSingleton<GatewaySchemaSpecCatalog>();
        services.AddSingleton<GatewayPublicasDocCatalog>();
        services.AddSingleton<GatewayOpenApiSchemaNormalizer>();
        services.AddSingleton<GatewayOpenApiMerger>();
        return services;
    }

    public static WebApplication UseGatewaySwaggerUi(this WebApplication app)
    {
        app.MapGet("/swagger/v1/swagger.json", async (
            HttpContext httpContext,
            GatewayOpenApiMerger merger,
            CancellationToken cancellationToken) =>
        {
            httpContext.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            var json = await merger.GetMergedJsonAsync(cancellationToken);
            return Results.Content(json, "application/json");
        });

        app.MapGet("/gateway-docs/{**path}", () => Results.Redirect("/swagger"));

        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "HotelLux API — Gateway";
            options.RoutePrefix = "swagger";
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelsExpandDepth(0);
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelLux API");

            options.InjectStylesheet("/swagger-custom.css");
        });

        app.MapGet("/swagger-custom.css", () => Results.Content(
            """
            .swagger-ui .opblock-tag { font-size: 1.05rem; border-bottom: 1px solid #e8e8e8; }
            .swagger-ui .opblock-tag[data-tag="Accommodations"],
            .swagger-ui .opblock-tag[data-tag="ReservasPublic"] {
              background: #f0f7ff;
            }
            """,
            "text/css"));

        return app;
    }
}
