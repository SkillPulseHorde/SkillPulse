using ReportService.Application.Models;

namespace ReportService.Application;

public interface IReportGenerator
{
    Task<byte[]> GenerateReportAsync(
        AssessmentResultModel assessmentResult,
        CompetencesAndCriteriaNamesModel names,
        RecommendationsModel recommendations,
        byte[] chartImage,
        string employeeName,
        CancellationToken ct = default);
}