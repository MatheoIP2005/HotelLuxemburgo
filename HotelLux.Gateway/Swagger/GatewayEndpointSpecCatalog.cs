using System.Text.RegularExpressions;

namespace HotelLux.Gateway.Swagger;

/// <summary>
/// Catálogo normativo leído de docs/endpoints_publicas.txt y docs/endpoints_locales.txt (líneas 1-368).
/// Los tags de Swagger coinciden exactamente con los nombres de sección del archivo locales.
/// </summary>
public sealed class GatewayEndpointSpecCatalog
{
    private static readonly string[] HttpMethods = ["GET", "POST", "PUT", "PATCH", "DELETE"];

    private readonly List<SpecEntry> _entries;
    private readonly Dictionary<string, TagInfo> _tags;
    private readonly Dictionary<string, int> _sectionOrder;

    public GatewayEndpointSpecCatalog(IWebHostEnvironment env, ILogger<GatewayEndpointSpecCatalog> logger)
    {
        var publicasPath = FindRepoFile(env, "endpoints_publicas.txt");
        var localesPath = FindRepoFile(env, "endpoints_locales.txt");

        _entries = [];
        _tags = new Dictionary<string, TagInfo>(StringComparer.OrdinalIgnoreCase);
        _sectionOrder = BuildSectionOrder(localesPath);

        LoadPublicas(publicasPath);
        LoadLocales(localesPath);

        logger.LogInformation(
            "Catálogo Swagger: {Count} operaciones ({Publicas} públicas, {Locales} locales), {Tags} secciones",
            _entries.Count,
            _entries.Count(e => e.Source == "publicas"),
            _entries.Count(e => e.Source == "locales"),
            _tags.Count);
    }

    public IReadOnlyList<SpecEntry> AllEntries => _entries;

    public bool TryGetSpec(string method, string path, out SpecEntry entry)
    {
        entry = _entries.FirstOrDefault(e =>
            e.Method.Equals(method, StringComparison.OrdinalIgnoreCase)
            && PathMatches(e.Path, path))!;

        return entry is not null;
    }

    public IReadOnlyList<TagInfo> GetOrderedTags(IEnumerable<string> usedTagNames)
    {
        var used = new HashSet<string>(usedTagNames, StringComparer.OrdinalIgnoreCase);
        return _tags.Values
            .Where(t => used.Contains(t.Name))
            .OrderBy(t => t.Order)
            .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void LoadPublicas(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            var m = Regex.Match(line.Trim(),
                @"^(GET|POST|PUT|PATCH|DELETE)\s+(/api/v1/.+)$",
                RegexOptions.IgnoreCase);
            if (!m.Success)
                continue;

            var path = m.Groups[2].Value;
            var tag = ResolvePublicasTag(path);
            RegisterTag(tag);
            AddEntry(m.Groups[1].Value.ToUpperInvariant(), path, tag, "publicas");
        }
    }

    private void LoadLocales(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var section = "General";
        string? pendingMethod = null;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line == "Schemas")
                break;

            if (HttpMethods.Contains(line, StringComparer.OrdinalIgnoreCase))
            {
                pendingMethod = line.ToUpperInvariant();
                continue;
            }

            if (line.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase) && pendingMethod is not null)
            {
                string? summary = null;
                if (i + 1 < lines.Length)
                {
                    var next = lines[i + 1].Trim();
                    if (IsLocalesSummaryLine(next))
                    {
                        summary = next;
                        i++;
                    }
                }

                RegisterTag(section);
                AddEntry(pendingMethod, line, section, "locales", summary);
                pendingMethod = null;
                continue;
            }

            if (pendingMethod is null
                && Regex.IsMatch(line, @"^[A-Z][A-Za-z0-9]+$"))
            {
                section = line;
            }
        }
    }

    private static bool IsLocalesSummaryLine(string line) =>
        !string.IsNullOrWhiteSpace(line)
        && !HttpMethods.Contains(line, StringComparer.OrdinalIgnoreCase)
        && !line.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase)
        && line != "Schemas";

    /// <summary>
    /// Tags para rutas de endpoints_publicas.txt usando nombres de sección de endpoints_locales.txt.
    /// </summary>
    private static string ResolvePublicasTag(string path)
    {
        if (path.Equals("/api/v1/public/reservas", StringComparison.OrdinalIgnoreCase))
            return "ReservasPublic";

        return "Accommodations";
    }

    private Dictionary<string, int> BuildSectionOrder(string localesPath)
    {
        var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lines = File.ReadAllLines(localesPath);
        var rank = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed == "Schemas")
                break;

            if (Regex.IsMatch(trimmed, @"^[A-Z][A-Za-z0-9]+$")
                && !HttpMethods.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                if (!order.ContainsKey(trimmed))
                {
                    rank += 10;
                    order[trimmed] = rank;
                }
            }
        }

        return order;
    }

    private void AddEntry(string method, string path, string tag, string source, string? summary = null)
    {
        var canonicalPath = CanonicalizePath(path);

        if (source == "locales"
            && _entries.Any(e => e.Source == "publicas"
                && e.Method == method
                && PathMatches(e.Path, canonicalPath)))
            return;

        if (_entries.Any(e => e.Method == method && PathMatches(e.Path, canonicalPath)))
            return;

        _entries.Add(new SpecEntry(method, canonicalPath, tag, source, summary));
    }

    private void RegisterTag(string sectionName)
    {
        if (_tags.ContainsKey(sectionName))
            return;

        var order = _sectionOrder.TryGetValue(sectionName, out var o) ? o : 900;
        _tags[sectionName] = new TagInfo(sectionName, order, string.Empty);
    }

    internal static string FindRepoFileStatic(IWebHostEnvironment env, string fileName) =>
        FindRepoFile(env, fileName);

    private static string FindRepoFile(IWebHostEnvironment env, string fileName)
    {
        var dir = env.ContentRootPath;
        for (var i = 0; i < 10; i++)
        {
            foreach (var candidate in new[]
                     {
                         System.IO.Path.Combine(dir, fileName),
                         System.IO.Path.Combine(dir, "docs", fileName)
                     })
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            var parent = Directory.GetParent(dir);
            if (parent is null)
                break;
            dir = parent.FullName;
        }

        throw new FileNotFoundException(
            $"No se encontró {fileName}. Debe estar en docs/ o en la raíz del repositorio.");
    }

    /// <summary>Ruta canónica con nombres de parámetro reales (para OpenAPI/Swagger UI).</summary>
    private static string CanonicalizePath(string path)
    {
        path = path.Trim().TrimEnd('/');
        if (!path.StartsWith('/'))
            path = "/" + path;
        return path;
    }

    /// <summary>Clave de comparación: segmentos dinámicos normalizados a <c>{}</c>.</summary>
    private static string NormalizePathForMatch(string path)
    {
        var segments = CanonicalizePath(path).Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].StartsWith('{') && segments[i].EndsWith('}'))
                segments[i] = "{}";
        }

        return "/" + string.Join('/', segments);
    }

    private static bool PathMatches(string pattern, string actual) =>
        string.Equals(NormalizePathForMatch(pattern), NormalizePathForMatch(actual), StringComparison.OrdinalIgnoreCase);

    public sealed record SpecEntry(string Method, string Path, string Tag, string Source, string? Summary = null);

    public sealed record TagInfo(string Name, int Order, string Description);
}
