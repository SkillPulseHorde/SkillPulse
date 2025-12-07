namespace RecommendationService.Infrastructure.Http.ServiceClientOptions;

public sealed class AssessmentServiceOptions
{
    public string BaseUrl { get; init; } = string.Empty;

    public string InternalToken { get; init; } = string.Empty;
}