using System.Text.Json.Nodes;

namespace HotelLux.Gateway.Swagger;

public static class GatewaySwaggerTagResolver
{
    public static JsonArray BuildOrderedTags(
        GatewayEndpointSpecCatalog catalog,
        IEnumerable<string> usedTagNames)
    {
        var used = new HashSet<string>(usedTagNames, StringComparer.OrdinalIgnoreCase);
        var tags = new JsonArray();

        foreach (var tag in catalog.GetOrderedTags(used))
        {
            tags.Add(new JsonObject
            {
                ["name"] = tag.Name,
                ["description"] = tag.Description
            });
        }

        return tags;
    }
}
