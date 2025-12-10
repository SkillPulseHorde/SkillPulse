namespace ReportService.Infrastructure.Dto;

public record ReportDto
{
    public required string EmployeeName { get; init; }
    public required byte[] Data { get; init; }
}