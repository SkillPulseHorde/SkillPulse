namespace ReportService.Application.Models;

public record ReportModel
{
    public required string FileName { get; init; }
    public required byte[] Data { get; init; }
}