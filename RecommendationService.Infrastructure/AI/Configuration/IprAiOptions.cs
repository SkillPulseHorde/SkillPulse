namespace RecommendationService.Infrastructure.AI.Configuration;

public sealed record IprAiOptions
{
    public required string ApiKey { get; set; }

    public required string BaseUrl { get; set; }

    public required string Model { get; set; }

    public double Temperature { get; set; } = 0.7;
}