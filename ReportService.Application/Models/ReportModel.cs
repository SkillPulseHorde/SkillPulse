namespace ReportService.Application.Models;

public record ReportModel
{
    public required string EmployeeName { get; init; }
    public required byte[] Data { get; init; }
}