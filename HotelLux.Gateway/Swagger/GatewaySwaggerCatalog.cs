namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Microservicios cuyo OpenAPI se agrega al documento unificado del Gateway.
/// </summary>
public static class GatewaySwaggerCatalog
{
    public sealed record ServiceDoc(
        string RouteKey,
        string ClusterId,
        string DisplayName,
        string TagName,
        string SwaggerRelativePath,
        string Hint);

    public static IReadOnlyList<ServiceDoc> Services { get; } =
    [
        new("auth", "auth", "Auth — autenticación y seguridad",
            "Auth",
            "/swagger/v1/swagger.json",
            "login, usuarios, roles, permisos · /api/v1/auth, /api/v1/internal/..."),

        new("accommodation", "accommodation", "Accommodation — alojamiento",
            "Accommodation",
            "/swagger/v1/swagger.json",
            "search, detalle, reviews, habitaciones · /api/v1/accommodations, /api/v1/internal/..."),

        new("reservation", "reservation", "Reservation — reservas y clientes",
            "Reservation",
            "/swagger/v1/swagger.json",
            "POST/GET reservas marketplace · /api/v1/accommodations/reservas, /api/v1/internal/reservas"),

        new("stay", "stay", "Stay — estadías, cargos y valoraciones",
            "Stay",
            "/swagger/v1/swagger.json",
            "/api/v1/internal/estadias, /api/v1/internal/valoraciones"),

        new("finance", "finance", "Finance — facturas y pagos",
            "Finance",
            "/swagger/v1/swagger.json",
            "/api/v1/internal/facturas, /api/v1/internal/pagos"),

        new("audit", "audit", "Audit — auditoría",
            "Audit",
            "/swagger/v1/swagger.json",
            "/api/v1/internal/auditoria")
    ];
}
