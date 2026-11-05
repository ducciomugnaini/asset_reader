using AssetReader.Application.Assets;
using AssetReader.Infrastructure.BorsaItaliana;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssetReader.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAssetReaderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BorsaItalianaOptions>(options =>
        {
            options.BaseUrl = configuration["BORSA_ITALIANA_BASE_URL"] ??
                              "https://grafici.borsaitaliana.it";
            options.Token = configuration["BORSA_ITALIANA_TOKEN"];
            options.TimeoutSeconds = int.TryParse(
                configuration["PROVIDER_TIMEOUT_SECONDS"],
                out var timeout)
                ? timeout
                : 10;
        });
        services.AddHttpClient<IAssetDataProvider, BorsaItalianaDataProvider>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<BorsaItalianaOptions>>()
                    .Value;
                BorsaItalianaDataProvider.Configure(client, options);
            });
        return services;
    }
}
