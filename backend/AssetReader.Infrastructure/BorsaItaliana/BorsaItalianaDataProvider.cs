using System.Net.Http.Headers;
using System.Text.Json;
using AssetReader.Application.Assets;
using AssetReader.Domain.Assets;
using Microsoft.Extensions.Logging;

namespace AssetReader.Infrastructure.BorsaItaliana;

public sealed class BorsaItalianaDataProvider(
    HttpClient httpClient,
    ILogger<BorsaItalianaDataProvider> logger) : IAssetDataProvider
{
    public async Task<AssetSnapshot> GetAsync(
        Isin isin,
        AssetPeriod period,
        CancellationToken cancellationToken)
    {
        var endpoint = BorsaItalianaEndpoint.Build(isin, period);
        var providerUrl = new Uri(httpClient.BaseAddress!, endpoint);
        logger.LogInformation("Calling Borsa Italiana provider URL: {ProviderUrl}", providerUrl);

        try
        {
            using var response = await httpClient.GetAsync(
                endpoint,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Borsa Italiana returned HTTP status {StatusCode} for period {Period}",
                    response.StatusCode,
                    period);
                throw new BorsaItalianaUnavailableException(
                    $"Provider returned HTTP {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);
            return BorsaItalianaPayloadMapper.Map(document.RootElement, isin, period);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BorsaItalianaUnavailableException("Provider request timed out.");
        }
        catch (JsonException exception)
        {
            throw new BorsaItalianaPayloadException(
                "Provider returned an unrecognizable payload.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new BorsaItalianaUnavailableException(
                "Provider request failed.",
                exception);
        }
    }

    public static void Configure(HttpClient client, BorsaItalianaOptions options)
    {
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 1, 60));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.Token);
        }
    }
}
