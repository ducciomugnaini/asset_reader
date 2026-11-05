using AssetReader.Domain.Assets;

namespace AssetReader.Tests.Assets;

public sealed class AssetPeriodTests
{
    [Theory]
    [InlineData("daily", AssetPeriod.Daily)]
    [InlineData("monthly", AssetPeriod.Monthly)]
    [InlineData("quarterly", AssetPeriod.Quarterly)]
    public void TryParse_SupportedValue_ReturnsExpectedPeriod(
        string value,
        AssetPeriod expected)
    {
        var result = AssetPeriodParser.TryParse(value, out var period);

        Assert.True(result);
        Assert.Equal(expected, period);
    }

    [Fact]
    public void TryParse_UnsupportedValue_ReturnsFalse()
    {
        var result = AssetPeriodParser.TryParse("weekly", out _);

        Assert.False(result);
    }
}
