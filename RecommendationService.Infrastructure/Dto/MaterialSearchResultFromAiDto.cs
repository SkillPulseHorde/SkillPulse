using System.Text.Json.Serialization;

namespace RecommendationService.Infrastructure.Dto;

public sealed record MaterialSearchResultFromAiDto
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("author")]
    public required string?  Author { get; init; }

    [JsonPropertyName("type")]
    public required string?  Type { get; init; }

    [JsonPropertyName("link")]
    public required string? Link { get; init; }

    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}