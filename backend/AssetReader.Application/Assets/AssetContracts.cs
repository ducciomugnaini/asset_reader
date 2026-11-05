using AssetReader.Domain.Assets;

namespace AssetReader.Application.Assets;

public sealed record AssetQueryRequest(string? Isin, string? Period);

public sealed record AssetPointResponse(DateTimeOffset Time, decimal Value);

public sealed record AssetResponse(
    string Isin,
    string Period,
    string? Name,
    string? Currency,
    IReadOnlyList<AssetPointResponse> Points);

public sealed record AssetQueryResult(
    AssetResponse? Response,
    string? ErrorCode,
    string? ErrorMessage)
{
    public bool IsSuccess => Response is not null;

    public static AssetQueryResult Invalid(string code, string message) =>
        new(null, code, message);

    public static AssetQueryResult Success(AssetResponse response) =>
        new(response, null, null);
}
