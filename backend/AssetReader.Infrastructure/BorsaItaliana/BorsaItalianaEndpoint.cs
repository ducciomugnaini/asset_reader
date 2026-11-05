using AssetReader.Domain.Assets;

namespace AssetReader.Infrastructure.BorsaItaliana;

internal static class BorsaItalianaEndpoint
{
    public static string Build(Isin isin, AssetPeriod period)
    {
        var instrument = $"{isin.Value},XMIL,ISIN";
        return period switch
        {
            AssetPeriod.Daily => $"api/instruments/{instrument}/intraday?resolution=1MN",
            AssetPeriod.Monthly => $"api/instruments/{instrument}/history/period?period=1M&adjustment=true&add-last-price=true",
            AssetPeriod.Quarterly => $"api/instruments/{instrument}/history/period?period=3M&adjustment=true&add-last-price=true",
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
    }
}
