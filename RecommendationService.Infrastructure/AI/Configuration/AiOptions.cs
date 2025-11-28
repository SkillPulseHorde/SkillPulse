namespace RecommendationService.Infrastructure.AI.Configuration;

public class AiOptions
{
    public required string ApiKey { get; set; }
    
    public required string BaseUrl { get; set; }
    
    public required string Model { get; set; }
    
    public double Temperature { get; set; } = 0.7;
}