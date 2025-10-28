namespace AssessmentService.Infrastructure.Dto;

public sealed record CheckUsersExistRequestDto
{
    public List<Guid> UserIds { get; init; } = [];
}
