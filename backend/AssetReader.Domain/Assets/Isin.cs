using System.Text.RegularExpressions;

namespace AssetReader.Domain.Assets;

public sealed partial record Isin
{
    public string Value { get; }

    private Isin(string value) => Value = value;

    public static bool TryCreate(string? value, out Isin? isin)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (normalized is not null && Pattern().IsMatch(normalized))
        {
            isin = new Isin(normalized);
            return true;
        }

        isin = null;
        return false;
    }

    [GeneratedRegex("^[A-Z]{2}[A-Z0-9]{9}[0-9]$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();
}
