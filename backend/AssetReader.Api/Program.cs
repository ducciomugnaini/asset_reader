using System.Threading.RateLimiting;
using AssetReader.Application.Assets;
using AssetReader.Infrastructure;
using AssetReader.Infrastructure.BorsaItaliana;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAssetReaderInfrastructure(builder.Configuration);
builder.Services.AddScoped<AssetQueryService>();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(builder.Configuration["FRONTEND_ORIGIN"] ?? "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("provider", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();
app.UseCors("Frontend");
app.UseRateLimiter();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/assets/{isin}", async (
    string isin,
    string? period,
    AssetQueryService assetQueryService,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await assetQueryService.QueryAsync(
            new AssetQueryRequest(isin, period),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Response)
            : Results.BadRequest(new ApiError(result.ErrorCode!, result.ErrorMessage!));
    }
    catch (BorsaItalianaPayloadException exception)
    {
        logger.LogWarning(exception, "Invalid payload received from Borsa Italiana.");
        return Results.Problem(
            statusCode: StatusCodes.Status502BadGateway,
            title: "Risposta dati non valida",
            detail: "Il servizio dati ha restituito una risposta non interpretabile.");
    }
    catch (BorsaItalianaUnavailableException exception)
    {
        logger.LogWarning(exception, "Borsa Italiana provider is unavailable.");
        return Results.Problem(
            statusCode: StatusCodes.Status502BadGateway,
            title: "Servizio dati non disponibile",
            detail: "Non è stato possibile recuperare i dati dell'asset. Riprova tra poco.");
    }
}).RequireRateLimiting("provider");

app.Run();

public sealed record ApiError(string Code, string Message);
