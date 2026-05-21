using System.Text.RegularExpressions;

namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Esquemas normativos (docs/endpoints_locales.txt, sección Schemas).
/// </summary>
public sealed class GatewaySchemaSpecCatalog
{
    private readonly Dictionary<string, SchemaDefinition> _definitions;

    public GatewaySchemaSpecCatalog(IWebHostEnvironment env)
    {
        var path = GatewayEndpointSpecCatalog.FindRepoFileStatic(env, "endpoints_locales.txt");
        _definitions = ParseSchemas(File.ReadAllLines(path));
    }

    public IReadOnlyDictionary<string, SchemaDefinition> Definitions => _definitions;

    public bool TryGet(string name, out SchemaDefinition definition) =>
        _definitions.TryGetValue(name, out definition!);

    private static Dictionary<string, SchemaDefinition> ParseSchemas(string[] lines)
    {
        var result = new Dictionary<string, SchemaDefinition>(StringComparer.OrdinalIgnoreCase);
        var inSchemas = false;
        SchemaDefinition? current = null;
        SchemaProperty? lastProp = null;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line == "Schemas")
            {
                inSchemas = true;
                continue;
            }

            if (!inSchemas)
                continue;

            var schemaStart = Regex.Match(line, @"^([A-Za-z][A-Za-z0-9]+)\{$");
            if (schemaStart.Success)
            {
                current = new SchemaDefinition(schemaStart.Groups[1].Value, []);
                result[current.Name] = current;
                lastProp = null;
                continue;
            }

            if (current is null)
                continue;

            if (line.StartsWith("nullable:", StringComparison.OrdinalIgnoreCase) && lastProp is not null)
            {
                lastProp.Nullable = true;
                continue;
            }

            if (line.StartsWith("description:", StringComparison.OrdinalIgnoreCase) && lastProp is not null)
            {
                lastProp.Description = line["description:".Length..].Trim();
                continue;
            }

            var propMatch = Regex.Match(line, @"^([A-Za-z][A-Za-z0-9_]+)\t(.+)$");
            if (!propMatch.Success)
                continue;

            var propName = propMatch.Groups[1].Value;
            var propValue = propMatch.Groups[2].Value.Trim();
            string? refSchema = null;
            if (propValue.EndsWith("{...}", StringComparison.Ordinal))
                refSchema = propValue[..^4];

            lastProp = new SchemaProperty(propName, refSchema, false, null);
            current.Properties.Add(lastProp);
        }

        return result;
    }

    public sealed class SchemaDefinition
    {
        public SchemaDefinition(string name, List<SchemaProperty> properties)
        {
            Name = name;
            Properties = properties;
        }

        public string Name { get; }
        public List<SchemaProperty> Properties { get; }
    }

    public sealed class SchemaProperty
    {
        public SchemaProperty(string name, string? refSchema, bool nullable, string? description)
        {
            Name = name;
            RefSchema = refSchema;
            Nullable = nullable;
            Description = description;
        }

        public string Name { get; }
        public string? RefSchema { get; }
        public bool Nullable { get; set; }
        public string? Description { get; set; }
    }
}

