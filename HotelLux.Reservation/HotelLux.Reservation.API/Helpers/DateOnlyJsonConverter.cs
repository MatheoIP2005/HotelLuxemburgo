using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotelLux.Reservation.API.Helpers;

/// <summary>
/// Converter que acepta tanto "yyyy-MM-dd" como strings ISO 8601 completos
/// ("2026-05-13T14:50:40.691Z") al deserializar un campo DateOnly.
/// Necesario porque el contrato público (endpoints_publicas.txt) envía datetime completos
/// pero los DTOs internos usan DateOnly.
/// </summary>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            throw new JsonException("Se esperaba una fecha válida pero se recibió un valor vacío.");

        // Intentar primero el formato nativo de DateOnly: "yyyy-MM-dd"
        if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateOnly))
            return dateOnly;

        // Si falla, intentar parsear como DateTime (ISO 8601 completo) y truncar a fecha
        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var dt))
            return DateOnly.FromDateTime(dt);

        throw new JsonException($"No se pudo convertir '{raw}' a DateOnly. " +
                                "Use el formato 'yyyy-MM-dd' o ISO 8601.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
