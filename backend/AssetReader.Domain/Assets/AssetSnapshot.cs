namespace AssetReader.Domain.Assets;

public sealed record AssetSnapshot(
    Isin Isin,
    AssetPeriod Period,
    string? Name,
    string? Currency,
    IReadOnlyList<AssetPoint> Points);
