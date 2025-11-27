using AssessmentService.Application.Models;

namespace AssessmentService.Api.Dto;

public sealed record CompetenceResultHistoryDto(
    Guid AssessmentId,
    DateTime AssessmentDate,
    double CompetenceScore);

public static class CompetenceResultHistoryDtoExtensions
{
    public static CompetenceResultHistoryDto ToDto(this CompetenceResultHistoryModel model)
    {
        return new CompetenceResultHistoryDto(
            model.AssessmentId,
            model.AssessmentDate,
            model.CompetenceScore);
    }

    public static List<CompetenceResultHistoryDto> ToDto(this List<CompetenceResultHistoryModel> models)
    {
        return models.Select(m => m.ToDto()).ToList();
    }
}
