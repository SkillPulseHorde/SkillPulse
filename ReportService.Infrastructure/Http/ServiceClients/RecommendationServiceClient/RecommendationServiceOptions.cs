namespace ReportService.Infrastructure.Http.ServiceClients.RecommendationServiceClient;

public class RecommendationServiceOptions
{
    public string BaseUrl { get; init; } = string.Empty;

    public string InternalToken { get; init; } = string.Empty;
}