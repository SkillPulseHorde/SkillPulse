namespace AssessmentService.Infrastructure.Dto;

public sealed record GetUsersByIdsRequestDto
{
    public List<Guid> UserIds { get; init; } = [];
}