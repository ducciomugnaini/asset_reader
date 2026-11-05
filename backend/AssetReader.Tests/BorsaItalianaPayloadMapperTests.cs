using System.Text.Json;
using AssetReader.Domain.Assets;
using AssetReader.Infrastructure.BorsaItaliana;

namespace AssetReader.Tests;

public sealed class BorsaItalianaPayloadMapperTests
{
    private static readonly Isin TestIsin = CreateIsin();

    [Fact]
    public void Map_DailyPayload_UsesEndPriceAndParsesIntradayTime()
    {
        using var document = JsonDocument.Parse("""
            {
              "intradayPoint": [
                { "time": "20261104-09:00:00", "beginPx": 96.12, "endPx": 96.11 }
              ]
            }
            """);

        var result = BorsaItalianaPayloadMapper.Map(
            document.RootElement,
            TestIsin,
            AssetPeriod.Daily);

        Assert.Single(result.Points);
        Assert.Equal(96.11m, result.Points[0].Value);
        Assert.Equal(9, result.Points[0].Time.Hour);
    }

    [Fact]
    public void Map_HistoryPayload_UsesLastPriceAndParsesDate()
    {
        using var document = JsonDocument.Parse("""
            {
              "history": {
                "historyDt": [
                  { "dt": "20261005", "closePx": 95.05, "lastPx": 95.13 }
                ]
              }
            }
            """);

        var result = BorsaItalianaPayloadMapper.Map(
            document.RootElement,
            TestIsin,
            AssetPeriod.Monthly);

        Assert.Single(result.Points);
        Assert.Equal(95.13m, result.Points[0].Value);
        Assert.Equal(new DateTime(2026, 10, 5), result.Points[0].Time.DateTime.Date);
    }

    private static Isin CreateIsin()
    {
        Isin.TryCreate("IT0005696338", out var isin);
        return isin!;
    }
}
