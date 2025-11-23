namespace AssessmentService.Api.Dto;

public sealed record EvaluateeShortInfoDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Position { get; init; }
    public string? TeamName { get; init; }
}