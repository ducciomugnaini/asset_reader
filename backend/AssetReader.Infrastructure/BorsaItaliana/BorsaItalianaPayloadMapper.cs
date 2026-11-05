using System.Globalization;
using System.Text.Json;
using AssetReader.Domain.Assets;

namespace AssetReader.Infrastructure.BorsaItaliana;

public static class BorsaItalianaPayloadMapper
{
    public static AssetSnapshot Map(JsonElement root, Isin isin, AssetPeriod period)
    {
        var points = period == AssetPeriod.Daily
            ? MapIntradayPoints(root)
            : MapHistoryPoints(root);

        return new AssetSnapshot(
            isin,
            period,
            FindString(root, "name", "description", "instrumentName"),
            FindString(root, "currency", "currencyCode"),
            points);
    }

    private static IReadOnlyList<AssetPoint> MapIntradayPoints(JsonElement root)
    {
        if (!root.TryGetProperty("intradayPoint", out var items) ||
            items.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return items.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => TryReadIntradayPoint(item, out var point) ? point : null)
            .Where(point => point is not null)
            .Select(point => point!)
            .ToArray();
    }

    private static IReadOnlyList<AssetPoint> MapHistoryPoints(JsonElement root)
    {
        if (!root.TryGetProperty("history", out var history) ||
            !history.TryGetProperty("historyDt", out var items) ||
            items.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return items.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => TryReadHistoryPoint(item, out var point) ? point : null)
            .Where(point => point is not null)
            .Select(point => point!)
            .ToArray();
    }

    private static bool TryReadIntradayPoint(JsonElement element, out AssetPoint point)
    {
        point = default!;
        if (!TryGetString(element, "time", out var rawTime) ||
            !TryParseTime(rawTime, "yyyyMMdd-HH:mm:ss", out var time) ||
            !TryGetDecimal(element, "endPx", out var value) &&
            !TryGetDecimal(element, "beginPx", out value))
        {
            return false;
        }

        point = new AssetPoint(time, value);
        return true;
    }

    private static bool TryReadHistoryPoint(JsonElement element, out AssetPoint point)
    {
        point = default!;
        if (!TryGetString(element, "dt", out var rawTime) ||
            !TryParseTime(rawTime, "yyyyMMdd", out var time) ||
            !TryGetDecimal(element, "lastPx", out var value) &&
            !TryGetDecimal(element, "closePx", out value))
        {
            return false;
        }

        point = new AssetPoint(time, value);
        return true;
    }

    private static bool TryParseTime(string rawValue, string format, out DateTimeOffset time) =>
        DateTimeOffset.TryParseExact(
            rawValue,
            format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out time);

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(property.GetString()))
        {
            value = property.GetString()!;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryGetDecimal(JsonElement element, string propertyName, out decimal value)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.TryGetDecimal(out value);
            }

            if (property.ValueKind == JsonValueKind.String)
            {
                return decimal.TryParse(
                    property.GetString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out value);
            }
        }

        value = default;
        return false;
    }

    private static string? FindString(JsonElement root, params string[] keys)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;
        foreach (var property in root.EnumerateObject())
        {
            if (keys.Contains(property.Name, StringComparer.OrdinalIgnoreCase) &&
                property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString();
            }
        }

        foreach (var property in root.EnumerateObject())
        {
            var value = FindString(property.Value, keys);
            if (value is not null) return value;
        }

        return null;
    }
}
