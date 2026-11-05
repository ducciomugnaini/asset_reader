using AssetReader.Domain.Assets;

namespace AssetReader.Tests.Assets;

public sealed class IsinTests
{
    [Fact]
    public void TryCreate_InputWithSpacesAndLowercase_ReturnsNormalizedIsin()
    {
        var result = Isin.TryCreate(" it0005696338 ", out var isin);

        Assert.True(result);
        Assert.Equal("IT0005696338", isin!.Value);
    }

    [Fact]
    public void TryCreate_InvalidFormat_ReturnsFalse()
    {
        var result = Isin.TryCreate("INVALID", out var isin);

        Assert.False(result);
        Assert.Null(isin);
    }
}
