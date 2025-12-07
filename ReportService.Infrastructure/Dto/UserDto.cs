namespace ReportService.Infrastructure.Dto;

public sealed record UserDto
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? MidName { get; init; }
    public required string TeamName { get; init; }
    public required string Position { get; init; }
    public required string Grade { get; init; }
}