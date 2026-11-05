namespace AssetReader.Domain.Assets;

public enum AssetPeriod
{
    Daily,
    Monthly,
    Quarterly
}

public static class AssetPeriodParser
{
    public static bool TryParse(string? value, out AssetPeriod period)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        period = normalized switch
        {
            "daily" => AssetPeriod.Daily,
            "monthly" => AssetPeriod.Monthly,
            "quarterly" => AssetPeriod.Quarterly,
            _ => default
        };
        return normalized is "daily" or "monthly" or "quarterly";
    }

    public static string ToContractValue(this AssetPeriod period) =>
        period switch
        {
            AssetPeriod.Daily => "daily",
            AssetPeriod.Monthly => "monthly",
            AssetPeriod.Quarterly => "quarterly",
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
}
