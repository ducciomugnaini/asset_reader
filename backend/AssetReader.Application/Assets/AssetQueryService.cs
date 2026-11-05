using AssetReader.Domain.Assets;

namespace AssetReader.Application.Assets;

public sealed class AssetQueryService(IAssetDataProvider provider)
{
    public async Task<AssetQueryResult> QueryAsync(
        AssetQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (!Isin.TryCreate(request.Isin, out var isin))
        {
            return AssetQueryResult.Invalid(
                "INVALID_ISIN",
                "Inserisci un codice ISIN valido di 12 caratteri.");
        }

        if (!AssetPeriodParser.TryParse(request.Period, out var period))
        {
            return AssetQueryResult.Invalid(
                "INVALID_PERIOD",
                "Intervallo non valido.");
        }

        var snapshot = await provider.GetAsync(isin!, period, cancellationToken);
        var response = new AssetResponse(
            snapshot.Isin.Value,
            snapshot.Period.ToContractValue(),
            snapshot.Name,
            snapshot.Currency,
            snapshot.Points
                .GroupBy(point => point.Time)
                .Select(group => group.Last())
                .OrderBy(point => point.Time)
                .Select(point => new AssetPointResponse(point.Time, point.Value))
                .ToArray());

        return AssetQueryResult.Success(response);
    }
}
