using System.Text.Json.Serialization;

namespace RecommendationService.Infrastructure.AI.Services;

public record GigaChatTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")] public long ExpiresAt { get; set; }
}