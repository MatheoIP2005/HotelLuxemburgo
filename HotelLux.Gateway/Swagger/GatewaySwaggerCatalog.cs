namespace HotelLux.Gateway.Swagger;

/// <summary>
/// OpenAPI expuesto por cada microservicio (vía proxy /gateway-docs/*).
/// Los servicios deben estar en ejecución (cada uno expone /swagger/v*/swagger.json).
/// </summary>
public static class GatewaySwaggerCatalog
{
    public sealed record ServiceDoc(
        string RouteKey,
        string DisplayName,
        string SwaggerRelativePath,
        string Hint);

    public static IReadOnlyList<ServiceDoc> Services { get; } =
    [
        new("auth", "Auth — autenticación y seguridad (locales)",
            "/swagger/v1/swagger.json",
            "login, usuarios, roles, permisos · /api/v1/auth, /api/v1/internal/..."),

        new("accommodation", "Accommodation — alojamiento (públicas + locales)",
            "/swagger/v1/swagger.json",
            "search, detalle, reviews, habitaciones · /api/v1/accommodations, /api/v1/internal/..."),

        new("reservation", "Reservation — reservas y clientes (públicas + locales)",
            "/swagger/v1/swagger.json",
            "POST/GET reservas marketplace · /api/v1/accomodations/reservas, /api/v1/internal/reservas"),

        new("stay", "Stay — estadías, cargos y valoraciones (locales)",
            "/swagger/v1/swagger.json",
            "/api/v1/internal/estadias, /api/v1/internal/valoraciones"),

        new("finance", "Finance — facturas y pagos (locales)",
            "/swagger/v1/swagger.json",
            "/api/v1/internal/facturas, /api/v1/internal/pagos"),

        new("audit", "Audit — auditoría (locales)",
            "/swagger/v1/swagger.json",
            "/api/v1/internal/auditoria")
    ];

    public static string ProxyPath(string routeKey, string swaggerRelativePath) =>
        $"/gateway-docs/{routeKey}{swaggerRelativePath}";
}
