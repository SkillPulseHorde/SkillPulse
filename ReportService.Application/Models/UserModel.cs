namespace ReportService.Application.Models;

public sealed record UserModel
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Position { get; init; }
    public string? TeamName { get; init; }
    public EmployeeGrade Grade { get; init; }
}

public enum EmployeeGrade
{
    NA = 0,
    J1 = 1,
    J2 = 2,
    J3 = 3,
    M1 = 4,
    M2 = 5,
    M3 = 6,
    S = 7
}