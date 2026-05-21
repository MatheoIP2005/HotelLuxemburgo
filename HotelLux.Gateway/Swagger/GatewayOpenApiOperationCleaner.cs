using System.Text.Json.Nodes;

namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Limpia summary/description de operaciones fusionadas (microservicios, sin descripciones monolito).
/// </summary>
internal static class GatewayOpenApiOperationCleaner
{
    public static void StripDocumentation(JsonObject operation)
    {
        operation.Remove("summary");
        operation.Remove("description");

        if (operation["parameters"] is not JsonArray parameters)
            return;

        foreach (var node in parameters)
        {
            if (node is JsonObject param)
                param.Remove("description");
        }
    }
}
