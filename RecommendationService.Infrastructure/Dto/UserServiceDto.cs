using RecommendationService.Application.Models;

namespace RecommendationService.Infrastructure.Dto;

public sealed record UserServiceDto
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? MidName { get; init; }
    public required string TeamName { get; init; }
    public Guid? ManagerId { get; init; }
    public required string Position { get; init; }
    public required string Grade { get; init; }

    public ShortUserModel ToShortUserModel()
    {
        return new ShortUserModel()
        {
            Id = Id,
            Position = Position,
            Grade = Grade,
            TeamName = TeamName,
            ManagerId = ManagerId
        };
    }
}
