namespace AssetReader.Infrastructure.BorsaItaliana;

public sealed class BorsaItalianaOptions
{
    public const string SectionName = "BorsaItaliana";
    public string BaseUrl { get; set; } = "https://grafici.borsaitaliana.it";
    public string? Token { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
}
