using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;

namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Fusiona OpenAPI de microservicios filtrando solo el catálogo de endpoints_publicas.txt y endpoints_locales.txt.
/// </summary>
public sealed class GatewayOpenApiMerger
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly GatewayEndpointSpecCatalog _catalog;
    private readonly GatewayOpenApiSchemaNormalizer _schemaNormalizer;
    private readonly GatewayPublicasDocCatalog _publicasDoc;
    private readonly ILogger<GatewayOpenApiMerger> _logger;

    public GatewayOpenApiMerger(
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        GatewayEndpointSpecCatalog catalog,
        GatewayOpenApiSchemaNormalizer schemaNormalizer,
        GatewayPublicasDocCatalog publicasDoc,
        ILogger<GatewayOpenApiMerger> logger)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _catalog = catalog;
        _schemaNormalizer = schemaNormalizer;
        _publicasDoc = publicasDoc;
        _logger = logger;
    }

    public async Task<string> GetMergedJsonAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("gateway.merged.openapi.v10", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await BuildMergedJsonAsync(cancellationToken);
        }) ?? "{}";
    }

    private async Task<string> BuildMergedJsonAsync(CancellationToken cancellationToken)
    {
        var merged = new JsonObject
        {
            ["openapi"] = "3.0.1",
            ["info"] = new JsonObject
            {
                ["title"] = "HotelLux — API Gateway",
                ["version"] = "v1",
                ["description"] = BuildDescription()
            },
            ["servers"] = new JsonArray
            {
                new JsonObject { ["url"] = GetGatewayServerUrl() }
            },
            ["paths"] = new JsonObject(),
            ["components"] = new JsonObject
            {
                ["schemas"] = new JsonObject(),
                ["securitySchemes"] = new JsonObject
                {
                    ["Bearer"] = new JsonObject
                    {
                        ["type"] = "http",
                        ["scheme"] = "bearer",
                        ["bearerFormat"] = "JWT",
                        ["description"] = "Token JWT: POST /api/v1/auth/login"
                    }
                }
            }
        };

        var paths = merged["paths"]!.AsObject();
        var schemas = merged["components"]!.AsObject()["schemas"]!.AsObject();
        var rawPrefixedSchemas = new JsonObject();
        var usedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var missingServices = new List<string>();
        var notInCatalog = 0;

        foreach (var service in GatewaySwaggerCatalog.Services)
        {
            var spec = await TryFetchSpecAsync(service, cancellationToken);
            if (spec is null)
            {
                missingServices.Add(service.DisplayName);
                continue;
            }

            var prefix = service.RouteKey;
            MergeSchemas(spec, prefix, schemas, rawPrefixedSchemas);
            notInCatalog += MergePaths(spec, prefix, paths, usedTags);
        }

        InjectMissingCatalogOperations(paths, usedTags);

        _schemaNormalizer.Apply(merged, rawPrefixedSchemas);

        merged["tags"] = GatewaySwaggerTagResolver.BuildOrderedTags(_catalog, usedTags);

        if (missingServices.Count > 0)
        {
            var info = merged["info"]!.AsObject();
            info["description"] = info["description"]!.GetValue<string>()
                + "\n\n**Servicios no disponibles:** " + string.Join(", ", missingServices);
        }

        _logger.LogInformation(
            "OpenAPI Gateway: {Paths} paths, {Skipped} operaciones omitidas (no están en publicas/locales)",
            paths.Count,
            notInCatalog);

        return merged.ToJsonString();
    }

    private async Task<JsonObject?> TryFetchSpecAsync(
        GatewaySwaggerCatalog.ServiceDoc service,
        CancellationToken cancellationToken)
    {
        var baseUrl = GetClusterBaseUrl(service.ClusterId);
        if (baseUrl is null)
            return null;

        var url = $"{baseUrl}{service.SwaggerRelativePath}";
        try
        {
            var client = _httpClientFactory.CreateClient(nameof(GatewayOpenApiMerger));
            client.Timeout = TimeSpan.FromSeconds(8);
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonNode.Parse(json)?.AsObject();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener OpenAPI de {Url}", url);
            return null;
        }
    }

    private int MergePaths(
        JsonObject spec,
        string prefix,
        JsonObject targetPaths,
        ISet<string> usedTags)
    {
        var skipped = 0;
        if (spec["paths"] is not JsonObject sourcePaths)
            return 0;

        foreach (var (path, pathItemNode) in sourcePaths)
        {
            if (pathItemNode is not JsonObject pathObj)
                continue;

            string? catalogPath = null;
            var mergedPathItem = new JsonObject();
            var hasOperation = false;

            foreach (var method in new[] { "get", "post", "put", "patch", "delete" })
            {
                if (pathObj[method] is not JsonObject operation)
                    continue;

                if (!_catalog.TryGetSpec(method.ToUpperInvariant(), path, out var specEntry))
                {
                    skipped++;
                    continue;
                }

                catalogPath ??= specEntry.Path;

                var clone = operation.DeepClone() as JsonObject ?? new JsonObject();
                RewriteSchemaRefs(clone, prefix);

                if (clone["operationId"] is JsonValue opIdVal
                    && opIdVal.TryGetValue<string>(out var opId)
                    && !string.IsNullOrWhiteSpace(opId))
                {
                    clone["operationId"] = $"{prefix}_{opId}";
                }

                clone["tags"] = new JsonArray { specEntry.Tag };
                usedTags.Add(specEntry.Tag);
                ApplySecurity(clone, method, specEntry.Path, specEntry);

                mergedPathItem[method] = clone;
                hasOperation = true;
            }

            if (!hasOperation || catalogPath is null)
                continue;

            if (targetPaths[catalogPath] is JsonObject existing)
            {
                foreach (var kv in mergedPathItem)
                    existing[kv.Key] = kv.Value?.DeepClone();
            }
            else
            {
                targetPaths[catalogPath] = mergedPathItem;
            }
        }

        return skipped;
    }

    private void InjectMissingCatalogOperations(JsonObject paths, ISet<string> usedTags)
    {
        foreach (var entry in _catalog.AllEntries)
        {
            var method = entry.Method.ToLowerInvariant();
            var catalogPath = entry.Path;

            if (paths[catalogPath] is JsonObject pathItem && pathItem[method] is JsonObject)
                continue;

            var operation = new JsonObject
            {
                ["tags"] = new JsonArray { entry.Tag }
            };

            if (entry.Source == "publicas"
                && _publicasDoc.TryGet(entry.Method, catalogPath, out var publicDoc))
            {
                GatewayPublicasOpenApiBuilder.EnrichStub(operation, entry, publicDoc);
            }
            else
            {
                operation["summary"] = $"{entry.Method} {catalogPath}";
                operation["description"] =
                    "Definido en el catálogo (`docs/endpoints_locales.txt`). " +
                    "Levantá el microservicio para cargar request/response completos.";
                operation["responses"] = new JsonObject
                {
                    ["default"] = new JsonObject { ["description"] = "Respuesta del microservicio" }
                };
            }

            ApplySecurity(operation, method, catalogPath, entry);

            var item = paths[catalogPath] as JsonObject ?? new JsonObject();
            item[method] = operation;
            paths[catalogPath] = item;
            usedTags.Add(entry.Tag);
        }
    }

    private static void ApplySecurity(
        JsonObject operation,
        string method,
        string path,
        GatewayEndpointSpecCatalog.SpecEntry specEntry)
    {
        if (path.StartsWith("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            operation["security"] = new JsonArray();
            return;
        }

        if (specEntry.Source != "publicas")
            return;

        var anonymousMarketplace =
            (method == "get" && path.Equals("/api/v1/accommodations/search", StringComparison.OrdinalIgnoreCase))
            || (method == "get" && path.Equals("/api/v1/accommodations/{sucursalGuid}", StringComparison.OrdinalIgnoreCase))
            || (method == "get" && path.Equals("/api/v1/accommodations/{sucursalGuid}/reviews", StringComparison.OrdinalIgnoreCase))
            || (method == "post" && path.Equals("/api/v1/accommodations/reservas", StringComparison.OrdinalIgnoreCase));

        if (anonymousMarketplace)
        {
            operation["security"] = new JsonArray();
            return;
        }

        if (method == "get"
            && path.Equals("/api/v1/accommodations/reservas/{reservaGuid}", StringComparison.OrdinalIgnoreCase))
        {
            operation["security"] = new JsonArray
            {
                new JsonObject { ["Bearer"] = new JsonArray() }
            };
        }
    }

    private static void MergeSchemas(
        JsonObject spec,
        string prefix,
        JsonObject targetSchemas,
        JsonObject rawPrefixedSchemas)
    {
        if (spec["components"]?["schemas"] is not JsonObject sourceSchemas)
            return;

        foreach (var (name, schemaNode) in sourceSchemas)
        {
            if (schemaNode is null)
                continue;

            var clone = schemaNode.DeepClone();
            RewriteSchemaRefs(clone, prefix);
            var key = $"{prefix}_{name}";
            targetSchemas[key] = clone;
            rawPrefixedSchemas[key] = clone?.DeepClone();
        }
    }

    private static void RewriteSchemaRefs(JsonNode? node, string prefix)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj.TryGetPropertyValue("$ref", out var refNode)
                    && refNode is JsonValue refVal
                    && refVal.TryGetValue<string>(out var refStr)
                    && refStr.StartsWith("#/components/schemas/", StringComparison.Ordinal))
                {
                    var schemaName = refStr["#/components/schemas/".Length..];
                    obj["$ref"] = $"#/components/schemas/{prefix}_{schemaName}";
                }

                foreach (var key in obj.Select(p => p.Key).ToList())
                    RewriteSchemaRefs(obj[key], prefix);
                break;

            case JsonArray arr:
                foreach (var item in arr)
                    RewriteSchemaRefs(item, prefix);
                break;
        }
    }

    private string? GetClusterBaseUrl(string clusterId) =>
        _config[$"ReverseProxy:Clusters:{clusterId}:Destinations:api:Address"]?.TrimEnd('/');

    private string GetGatewayServerUrl()
    {
        var kestrel = _config["Kestrel:Endpoints:Http:Url"];
        if (!string.IsNullOrWhiteSpace(kestrel))
            return kestrel.Replace("0.0.0.0", "127.0.0.1").TrimEnd('/');

        return "http://127.0.0.1:5000";
    }

    private static string BuildDescription() =>
        """
        API unificada del Gateway (`http://127.0.0.1:5000`).

        Secciones alineadas con `docs/endpoints_locales.txt`. Los **5 endpoints públicos** de `docs/endpoints_publicas.txt` aparecen aunque Accommodation/Reservation estén apagados. Recargá con Ctrl+F5.
        """;
}
