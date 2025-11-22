using AssessmentService.Domain.Entities;

namespace AssessmentService.Application.Models;

public sealed record UserModel
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Position { get; init; }
    public string? TeamName { get; init; }
    public EmployeeGrade Grade { get; init; }
}