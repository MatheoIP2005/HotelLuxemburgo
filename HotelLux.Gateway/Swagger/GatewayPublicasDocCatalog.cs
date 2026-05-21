using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Ejemplos y metadatos de docs/endpoints_publicas.txt para enriquecer Swagger sin levantar microservicios.
/// </summary>
public sealed class GatewayPublicasDocCatalog
{
    private readonly Dictionary<string, PublicOperationDoc> _docs;

    public GatewayPublicasDocCatalog(IWebHostEnvironment env)
    {
        var path = GatewayEndpointSpecCatalog.FindRepoFileStatic(env, "endpoints_publicas.txt");
        _docs = Parse(File.ReadAllLines(path));
    }

    public bool TryGet(string method, string path, out PublicOperationDoc doc)
    {
        var key = $"{method.ToUpperInvariant()} {path}";
        return _docs.TryGetValue(key, out doc!);
    }

    private static Dictionary<string, PublicOperationDoc> Parse(string[] lines)
    {
        var result = new Dictionary<string, PublicOperationDoc>(StringComparer.OrdinalIgnoreCase);
        string? pendingMethod = null;
        string? pendingPath = null;
        var jsonLines = new List<string>();

        void Flush()
        {
            if (pendingMethod is null || pendingPath is null)
                return;

            var key = $"{pendingMethod} {pendingPath}";
            var json = string.Join('\n', jsonLines).Trim();
            JsonNode? example = null;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try { example = JsonNode.Parse(json); }
                catch { /* ignorar JSON malformado */ }
            }

            result[key] = new PublicOperationDoc(
                pendingMethod,
                pendingPath,
                ResolveResponseSchema(pendingMethod, pendingPath),
                ResolveRequestSchema(pendingMethod, pendingPath),
                pendingMethod == "POST" ? example : null,
                pendingMethod != "POST" ? example : null,
                ResolveParameters(pendingMethod, pendingPath));

            pendingMethod = null;
            pendingPath = null;
            jsonLines.Clear();
        }

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            var m = Regex.Match(line, @"^(GET|POST|PUT|PATCH|DELETE)\s+(/api/v1/.+)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                Flush();
                pendingMethod = m.Groups[1].Value.ToUpperInvariant();
                pendingPath = m.Groups[2].Value;
                continue;
            }

            if (pendingPath is not null)
                jsonLines.Add(raw);
        }

        Flush();
        return result;
    }

    private static string? ResolveResponseSchema(string method, string path) => (method, path) switch
    {
        ("GET", "/api/v1/accommodations/search") => "AccommodationSearchItemDtoPagedResponse",
        ("GET", "/api/v1/accommodations/{sucursalGuid}") => "AccommodationDetailResponse",
        ("GET", "/api/v1/accommodations/{sucursalGuid}/reviews") => "AccommodationReviewDtoPagedResponse",
        ("GET", "/api/v1/accommodations/reservas/{reservaGuid}") => "ReservaPublicDto",
        ("POST", "/api/v1/accommodations/reservas") => "ReservaPublicDto",
        _ => null
    };

    private static string? ResolveRequestSchema(string method, string path) => (method, path) switch
    {
        ("POST", "/api/v1/accommodations/reservas") => "CrearReservaRequest",
        _ => null
    };

    private static IReadOnlyList<PublicParameterDoc> ResolveParameters(string method, string path)
    {
        if (path == "/api/v1/accommodations/search")
        {
            return
            [
                Query("destino", "string", "Ciudad o ubicación"),
                Query("fechaInicio", "string", "date-time", "Inicio del rango (con fechaFin)"),
                Query("fechaFin", "string", "date-time", "Fin del rango (con fechaInicio)"),
                Query("num_adultos", "integer", "Adultos"),
                Query("num_habitaciones", "integer", "Habitaciones"),
                Query("pagina", "integer", "Página (default 1)"),
                Query("limite", "integer", "Tamaño de página (default 20)")
            ];
        }

        if (path == "/api/v1/accommodations/{sucursalGuid}")
        {
            return
            [
                Path("sucursalGuid", "uuid", "Identificador de la propiedad"),
                Query("fechaInicio", "string", "date-time", "Opcional — disponibilidad por tipo"),
                Query("fechaFin", "string", "date-time", "Opcional — disponibilidad por tipo")
            ];
        }

        if (path == "/api/v1/accommodations/{sucursalGuid}/reviews")
        {
            return
            [
                Path("sucursalGuid", "uuid", "Identificador de la propiedad"),
                Query("pagina", "integer", "Página"),
                Query("limite", "integer", "Tamaño de página")
            ];
        }

        if (path == "/api/v1/accommodations/reservas/{reservaGuid}")
            return [Path("reservaGuid", "uuid", "Identificador de la reserva")];

        return Array.Empty<PublicParameterDoc>();
    }

    private static PublicParameterDoc Query(string name, string type, string? description = null, string? format = null) =>
        new("query", name, type, format, false, description);

    private static PublicParameterDoc Path(string name, string format, string? description = null) =>
        new("path", name, "string", format, true, description);

    public sealed record PublicOperationDoc(
        string Method,
        string Path,
        string? ResponseSchema,
        string? RequestSchema,
        JsonNode? RequestExample,
        JsonNode? ResponseExample,
        IReadOnlyList<PublicParameterDoc> Parameters);

    public sealed record PublicParameterDoc(
        string In,
        string Name,
        string Type,
        string? Format,
        bool Required,
        string? Description);
}
