using System.Text.Json.Serialization;

namespace RecommendationService.Infrastructure.Dto;

public sealed record GigaChatTokenResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }
}