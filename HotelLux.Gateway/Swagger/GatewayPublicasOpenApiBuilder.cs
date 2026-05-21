using System.Text.Json.Nodes;

namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Construye operaciones OpenAPI de catálogo para endpoints públicos (sin depender de microservicios).
/// </summary>
public static class GatewayPublicasOpenApiBuilder
{
    public static void EnrichStub(
        JsonObject operation,
        GatewayEndpointSpecCatalog.SpecEntry entry,
        GatewayPublicasDocCatalog.PublicOperationDoc? doc)
    {
        operation["summary"] = $"{entry.Method} {entry.Path}";
        operation["description"] =
            "Endpoint público documentado en `docs/endpoints_publicas.txt`. " +
            "Disponible en el Gateway; levantá Accommodation/Reservation para esquemas enriquecidos desde el servicio.";

        if (doc is not null)
        {
            if (doc.Parameters.Count > 0)
                operation["parameters"] = BuildParameters(doc.Parameters);

            if (entry.Method == "POST" && (doc.RequestSchema is not null || doc.RequestExample is not null))
                operation["requestBody"] = BuildRequestBody(doc);

            operation["responses"] = BuildResponses(entry.Method, doc);
        }
        else
        {
            operation["responses"] = new JsonObject
            {
                ["200"] = new JsonObject { ["description"] = "OK" }
            };
        }
    }

    private static JsonArray BuildParameters(IReadOnlyList<GatewayPublicasDocCatalog.PublicParameterDoc> parameters)
    {
        var arr = new JsonArray();
        foreach (var p in parameters)
        {
            var schema = new JsonObject { ["type"] = p.Type };
            if (!string.IsNullOrWhiteSpace(p.Format))
                schema["format"] = p.Format;

            arr.Add(new JsonObject
            {
                ["name"] = p.Name,
                ["in"] = p.In,
                ["required"] = p.Required,
                ["description"] = p.Description ?? "",
                ["schema"] = schema
            });
        }

        return arr;
    }

    private static JsonObject BuildRequestBody(GatewayPublicasDocCatalog.PublicOperationDoc doc)
    {
        var media = new JsonObject
        {
            ["application/json"] = new JsonObject()
        };
        var content = media["application/json"]!.AsObject();

        if (doc.RequestSchema is not null)
        {
            content["schema"] = new JsonObject
            {
                ["$ref"] = $"#/components/schemas/{doc.RequestSchema}"
            };
        }

        if (doc.RequestExample is not null)
            content["example"] = doc.RequestExample.DeepClone();

        return new JsonObject
        {
            ["required"] = true,
            ["content"] = media
        };
    }

    private static JsonObject BuildResponses(string method, GatewayPublicasDocCatalog.PublicOperationDoc doc)
    {
        var responses = new JsonObject();
        var successCode = method == "POST" ? "201" : "200";
        var success = new JsonObject { ["description"] = method == "POST" ? "Creado" : "OK" };
        var media = new JsonObject { ["application/json"] = new JsonObject() };
        var content = media["application/json"]!.AsObject();

        if (doc.ResponseSchema is not null)
        {
            content["schema"] = new JsonObject
            {
                ["$ref"] = $"#/components/schemas/{doc.ResponseSchema}"
            };
        }

        if (doc.ResponseExample is not null)
            content["example"] = doc.ResponseExample.DeepClone();

        if (content.Count > 0)
            success["content"] = media;

        responses[successCode] = success;
        responses["400"] = new JsonObject { ["description"] = "Parámetros inválidos" };
        responses["404"] = new JsonObject { ["description"] = "No encontrado" };

        if (method == "GET" && doc.Path.Contains("/reservas/", StringComparison.Ordinal))
            responses["401"] = new JsonObject { ["description"] = "Requiere JWT" };

        return responses;
    }
}
