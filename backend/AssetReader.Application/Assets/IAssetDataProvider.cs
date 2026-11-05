using AssetReader.Domain.Assets;

namespace AssetReader.Application.Assets;

public interface IAssetDataProvider
{
    Task<AssetSnapshot> GetAsync(
        Isin isin,
        AssetPeriod period,
        CancellationToken cancellationToken);
}
