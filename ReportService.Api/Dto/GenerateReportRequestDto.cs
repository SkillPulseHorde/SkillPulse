namespace ReportService.Api.Dto;

public sealed record GenerateReportRequestDto
{
    public required Guid AssessmentId { get; init; }
    
    public required Guid EmployeeId { get; init; }
    
    public required IFormFile ChartImage { get; init; }
}