namespace RecommendationService.Infrastructure.Dto;

public record GetUsersByIdsRequestDto
{
    public required List<Guid> UserIds { get; init; }
}