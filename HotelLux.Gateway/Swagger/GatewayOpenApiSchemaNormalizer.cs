using System.Text.Json.Nodes;

namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Reemplaza components.schemas del OpenAPI fusionado por los nombres y propiedades de endpoints_locales.txt (Schemas).
/// </summary>
public sealed class GatewayOpenApiSchemaNormalizer
{
    private readonly GatewaySchemaSpecCatalog _catalog;

    private static readonly Dictionary<string, string> TypeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LoginRequestDTO"] = "LoginRequest",
        ["LoginResponseDTO"] = "LoginResponse",
        ["LoginSuccessResponse"] = "LoginResponseApiResponse",
        ["LoginSuccessData"] = "LoginResponse",
        ["RefreshRequestDTO"] = "RefreshTokenRequest",
        ["CambiarPasswordDTO"] = "CambiarPasswordRequest",
        ["FacturaDto"] = "FacturaResponse",
        ["PagoCreateDto"] = "CrearPagoRequest",
        ["AnularFacturaDto"] = "AnularFacturaBody",
        ["PagoEstadoDto"] = "CambiarEstadoPagoBody",
        ["GenerarFacturaRequestDto"] = "GenerarFacturaBody",
        ["FacturaLineaGeneracionDto"] = "FacturaDetalleResponse",
        ["ClienteDto"] = "ClienteResponse",
        ["ClienteCreateDto"] = "CrearClienteRequest",
        ["ClienteUpdateDto"] = "ActualizarClienteRequest",
        ["InhabilitarDto"] = "DeleteRequest",
        ["ReservaDTO"] = "ReservaResponse",
        ["ReservaCreateDTO"] = "CrearReservaRequest",
        ["CrearReservaPublicRequest"] = "CrearReservaPublicRequest",
        ["ReservaHabitacionPublicRequest"] = "ReservaHabitacionPublicRequest",
        ["ReservaHabitacionDTO"] = "ReservaHabitacionResponse",
        ["ReservaHabitacionCreateDTO"] = "ReservaHabitacionRequest",
        ["ReservaFiltroDTO"] = "ReservaResponse",
        ["PagedResultDTO"] = "ReservaResponsePaginatedResponse",
        ["UsuarioDTO"] = "UsuarioResponse",
        ["UsuarioCreateDTO"] = "CrearUsuarioRequest",
        ["UsuarioUpdateDTO"] = "ActualizarUsuarioRequest",
        ["RolDTO"] = "RolResponse",
        ["RolCreateDTO"] = "CrearRolRequest",
        ["RolUpdateDTO"] = "RolResponse",
        ["CatalogoServicioDTO"] = "CatalogoResponse",
        ["CatalogoServicioCreateDTO"] = "CrearCatalogoRequest",
        ["CatalogoServicioUpdateDTO"] = "ActualizarCatalogoRequest",
        ["SucursalDTO"] = "SucursalResponse",
        ["SucursalCreateDTO"] = "CrearSucursalRequest",
        ["SucursalUpdateDTO"] = "ActualizarSucursalRequest",
        ["SucursalPoliticasPatchDTO"] = "ActualizarSucursalRequest",
        ["HabitacionDTO"] = "HabitacionResponse",
        ["HabitacionCreateDTO"] = "CrearHabitacionRequest",
        ["HabitacionUpdateDTO"] = "ActualizarHabitacionRequest",
        ["TarifaDTO"] = "TarifaResponse",
        ["TarifaCreateDTO"] = "CrearTarifaRequest",
        ["TarifaUpdateDTO"] = "ActualizarTarifaRequest",
        ["TipoHabitacionDTO"] = "TipoHabitacionResponse",
        ["TipoHabitacionCreateDTO"] = "CrearTipoHabitacionRequest",
        ["TipoHabitacionUpdateDTO"] = "ActualizarTipoHabitacionRequest",
        ["SucursalImagenDTO"] = "SucursalImagenResponse",
        ["SucursalImagenCreateDTO"] = "CrearSucursalImagenRequest",
        ["AuditoriaEventoDetalleDto"] = "AuditoriaResponse",
        ["ValoracionCreateDto"] = "CrearValoracionRequest",
        ["EstadiaDto"] = "EstadiaResponse",
        ["CargoEstadiaDto"] = "CargoEstadiaResponse",
        ["ApiResponse"] = "ObjectApiResponse",
        ["ApiErrorResponse"] = "ApiErrorResponse",
        ["ProblemDetails"] = "ProblemDetails",
        ["StayReviewDto"] = "AccommodationReviewDto",
        ["StayRatingSummary"] = "ObjectApiResponse",
        ["StayValoracionClienteDto"] = "ValoracionResponse",
        ["ClienteInlineDTO"] = "ClientePublicRequest",
    };

    public GatewayOpenApiSchemaNormalizer(GatewaySchemaSpecCatalog catalog) => _catalog = catalog;

    public void Apply(JsonObject merged, JsonObject rawPrefixedSchemas)
    {
        var aliasToCanonical = BuildAliasMap(rawPrefixedSchemas);
        var canonical = new JsonObject();

        foreach (var (name, def) in _catalog.Definitions)
        {
            canonical[name] = BuildSchema(def, rawPrefixedSchemas, aliasToCanonical);
        }

        merged["components"]!.AsObject()["schemas"] = canonical;
        RewriteAllRefs(merged, aliasToCanonical);
    }

    private Dictionary<string, string> BuildAliasMap(JsonObject rawPrefixedSchemas)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in rawPrefixedSchemas.Select(p => p.Key))
        {
            var localName = ExtractLocalSchemaName(key);
            if (_catalog.TryGet(localName, out _))
            {
                map[key] = localName;
                continue;
            }

            if (TypeAliases.TryGetValue(localName, out var canonical) && _catalog.TryGet(canonical, out _))
            {
                map[key] = canonical;
            }
        }

        foreach (var (alias, canonical) in TypeAliases)
        {
            if (_catalog.TryGet(canonical, out _))
                map.TryAdd(alias, canonical);
        }

        return map;
    }

    private JsonObject BuildSchema(
        GatewaySchemaSpecCatalog.SchemaDefinition def,
        JsonObject rawPrefixedSchemas,
        IReadOnlyDictionary<string, string> aliasToCanonical)
    {
        JsonObject? source = null;
        if (_catalog.TryGet(def.Name, out _))
        {
            if (rawPrefixedSchemas.TryGetPropertyValue(def.Name, out var direct) && direct is JsonObject d)
                source = d;
            else
            {
                var fromAlias = aliasToCanonical
                    .FirstOrDefault(kv => kv.Value.Equals(def.Name, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(fromAlias.Key)
                    && rawPrefixedSchemas[fromAlias.Key] is JsonObject aliased)
                    source = aliased;
            }
        }

        var schema = new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() };
        var props = schema["properties"]!.AsObject();

        foreach (var prop in def.Properties)
        {
            if (prop.RefSchema is not null && _catalog.TryGet(prop.RefSchema, out var nested))
            {
                props[prop.Name] = new JsonObject
                {
                    ["$ref"] = $"#/components/schemas/{prop.RefSchema}"
                };
            }
            else if (TryBuildArrayItemsRef(def.Name, prop.Name, out var itemsRef) && _catalog.TryGet(itemsRef, out _))
            {
                props[prop.Name] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["$ref"] = $"#/components/schemas/{itemsRef}" }
                };
            }
            else if (TryBuildDataRef(def.Name, prop.Name, out var dataRef) && _catalog.TryGet(dataRef, out _))
            {
                props[prop.Name] = new JsonObject
                {
                    ["$ref"] = $"#/components/schemas/{dataRef}"
                };
            }
            else
            {
                props[prop.Name] = InferPropertySchema(prop.Name, source);
            }

            if (prop.Nullable && props[prop.Name] is JsonObject p)
                p["nullable"] = true;
        }

        return schema;
    }

    private static bool TryBuildArrayItemsRef(string schemaName, string propName, out string itemsRef)
    {
        itemsRef = string.Empty;
        if (!propName.Equals("items", StringComparison.OrdinalIgnoreCase))
            return false;

        if (schemaName.EndsWith("PagedResponse", StringComparison.Ordinal))
        {
            itemsRef = schemaName[..^"PagedResponse".Length];
            return true;
        }

        if (schemaName.EndsWith("PaginatedResponse", StringComparison.Ordinal))
        {
            itemsRef = schemaName[..^"PaginatedResponse".Length];
            return true;
        }

        if (schemaName.Equals("FacturaResponseListApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("FacturaDetalleResponseListApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("PagoResponseListApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("RolResponseListApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("CargoEstadiaResponseListApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("HabitacionResponseIReadOnlyListApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("SucursalImagenResponseListApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("StringListApiResponse", StringComparison.OrdinalIgnoreCase))
        {
            itemsRef = schemaName switch
            {
                _ when schemaName.StartsWith("FacturaDetalle") => "FacturaDetalleResponse",
                _ when schemaName.StartsWith("Factura") => "FacturaResponse",
                _ when schemaName.StartsWith("Pago") => "PagoResponse",
                _ when schemaName.StartsWith("Rol") => "RolResponse",
                _ when schemaName.StartsWith("CargoEstadia") => "CargoEstadiaResponse",
                _ when schemaName.StartsWith("Habitacion") => "HabitacionResponse",
                _ when schemaName.StartsWith("SucursalImagen") => "SucursalImagenResponse",
                _ => "string"
            };
            return true;
        }

        return false;
    }

    private static bool TryBuildDataRef(string schemaName, string propName, out string dataRef)
    {
        dataRef = string.Empty;
        if (!propName.Equals("data", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!schemaName.EndsWith("ApiResponse", StringComparison.Ordinal))
            return false;

        dataRef = schemaName[..^"ApiResponse".Length];
        if (schemaName.Equals("ObjectApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("StringApiResponse", StringComparison.OrdinalIgnoreCase)
            || schemaName.Equals("StringListApiResponse", StringComparison.OrdinalIgnoreCase))
            return false;

        return dataRef.Length > 0;
    }

    private static JsonObject InferPropertySchema(string propName, JsonObject? source)
    {
        if (source?["properties"] is JsonObject sourceProps
            && sourceProps.TryGetPropertyValue(propName, out var existing)
            && existing is JsonObject existingObj)
        {
            return existingObj.DeepClone()!.AsObject();
        }

        var lower = propName.ToLowerInvariant();
        if (lower.Contains("guid") || lower.EndsWith("uuid"))
            return new JsonObject { ["type"] = "string", ["format"] = "uuid" };
        if (lower.StartsWith("fecha") || lower.EndsWith("utc") || lower.Contains("expiration"))
            return new JsonObject { ["type"] = "string", ["format"] = "date-time" };
        if (lower.StartsWith("es") || lower.StartsWith("tiene") || lower.Contains("activo")
            || lower.Contains("permite") || lower.Contains("aplica") || lower.Contains("disponible")
            || lower.Contains("requiere") || lower.Contains("publicad"))
            return new JsonObject { ["type"] = "boolean" };
        if (lower.StartsWith("num") || lower.StartsWith("id") || lower.Contains("pagina")
            || lower.Contains("limite") || lower.Contains("total") || lower.Contains("puntuacion")
            || lower.Contains("estrellas") || lower.Contains("noches") || lower.Contains("orden")
            || lower.Contains("version") || lower.Contains("bytes") || lower.Contains("width")
            || lower.Contains("height") || lower.Contains("area"))
            return new JsonObject { ["type"] = "integer", ["format"] = "int32" };
        if (lower.Contains("precio") || lower.Contains("monto") || lower.Contains("subtotal")
            || lower.Contains("total") || lower.Contains("saldo") || lower.Contains("valor")
            || lower.Contains("descuento") || lower.Contains("latitud") || lower.Contains("longitud")
            || lower.Contains("promedio"))
            return new JsonObject { ["type"] = "number", ["format"] = "double" };
        if (lower is "items" or "roles" or "habitaciones" or "detalles" or "amenities"
            or "imagenes" or "serviciosdestacados" or "errors" or "data")
            return new JsonObject { ["type"] = "array", ["items"] = new JsonObject() };
        if (lower is "disponiblesenrango")
            return new JsonObject { ["type"] = "integer", ["format"] = "int32" };

        return new JsonObject { ["type"] = "string" };
    }

    private static void RewriteAllRefs(JsonObject merged, IReadOnlyDictionary<string, string> aliasToCanonical)
    {
        RewriteRefs(merged, aliasToCanonical);
    }

    private static void RewriteRefs(JsonNode? node, IReadOnlyDictionary<string, string> aliasToCanonical)
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
                    if (aliasToCanonical.TryGetValue(schemaName, out var canonical))
                        obj["$ref"] = $"#/components/schemas/{canonical}";
                    else if (TypeAliases.TryGetValue(schemaName, out var mapped))
                        obj["$ref"] = $"#/components/schemas/{mapped}";
                    else
                    {
                        var local = ExtractLocalSchemaName(schemaName);
                        if (TypeAliases.TryGetValue(local, out var mappedLocal))
                            obj["$ref"] = $"#/components/schemas/{mappedLocal}";
                        else if (aliasToCanonical.TryGetValue(local, out var canonicalLocal))
                            obj["$ref"] = $"#/components/schemas/{canonicalLocal}";
                        else if (TypeAliases.TryGetValue(schemaName, out var mappedFull))
                            obj["$ref"] = $"#/components/schemas/{mappedFull}";
                    }
                }

                foreach (var key in obj.Select(p => p.Key).ToList())
                    RewriteRefs(obj[key], aliasToCanonical);
                break;

            case JsonArray arr:
                foreach (var item in arr)
                    RewriteRefs(item, aliasToCanonical);
                break;
        }
    }

    private static string ExtractLocalSchemaName(string schemaName)
    {
        var local = schemaName.Contains('_')
            ? schemaName[(schemaName.IndexOf('_') + 1)..]
            : schemaName;

        var lastDot = local.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < local.Length - 1)
            local = local[(lastDot + 1)..];

        return local;
    }
}
