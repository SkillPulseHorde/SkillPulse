namespace UserService.Dto;

public sealed record GetUsersByIdsRequestDto
{
    public List<Guid> UserIds { get; init; } = [];
}