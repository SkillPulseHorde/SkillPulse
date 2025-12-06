namespace AuthService.Infrastructure.Dto;

public record GetUsersByIdsRequestDto
{
    public required List<Guid> UserIds { get; init; }
}