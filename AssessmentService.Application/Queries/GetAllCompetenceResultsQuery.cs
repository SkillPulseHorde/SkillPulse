using AssessmentService.Application.Models;
using AssessmentService.Domain;
using AssessmentService.Domain.Repos;
using Common;
using MediatR;

namespace AssessmentService.Application.Queries;

public sealed record GetAllCompetenceResultsQuery(Guid UserId, Guid CompetenceId) 
    : IRequest<Result<List<CompetenceResultHistoryModel>>>;

public sealed class GetAllCompetenceResultsQueryHandler(
    IAssessmentRepository assessmentRepository,
    IAssessmentResultRepository assessmentResultRepository,
    IEvaluationAnalyzer evaluationAnalyzer)
    : IRequestHandler<GetAllCompetenceResultsQuery, Result<List<CompetenceResultHistoryModel>>>
{
    public async Task<Result<List<CompetenceResultHistoryModel>>> Handle(
        GetAllCompetenceResultsQuery request, 
        CancellationToken ct)
    {
        // Получаем все завершенные оценки для пользователя
        var completedAssessments = await assessmentRepository
            .GetCompletedAssessmentsByEvaluateeIdReadonlyAsync(request.UserId, ct);

        if (completedAssessments.Count == 0)
            return new List<CompetenceResultHistoryModel>();

        var assessmentIds = completedAssessments.Select(a => a.Id).ToList();

        // Получаем существующие результаты
        var existingResults = await assessmentResultRepository
            .GetByAssessmentIdsAsync(assessmentIds, ct);
        
        var existingResultsDict = existingResults.ToDictionary(r => r.AssessmentId);

        // Определяем, для каких оценок нужно создать результаты
        var missingAssessmentIds = assessmentIds
            .Where(id => !existingResultsDict.ContainsKey(id))
            .ToList();

        // Создаем недостающие результаты 
        if (missingAssessmentIds.Count > 0)
        {
            var newResults = await evaluationAnalyzer
                .CreateAssessmentResultsAsync(missingAssessmentIds, ct);

            // Добавляем новые результаты в словарь
            foreach (var result in newResults)
            {
                existingResultsDict[result.AssessmentId] = result;
            }
        }

        // Формируем историю результатов по компетенции
        var history = completedAssessments
            .Select(assessment =>
            {
                if (!existingResultsDict.TryGetValue(assessment.Id, out var result))
                    return null;

                if (!result.Data.CompetenceSummaries.TryGetValue(request.CompetenceId, out var competenceSummary))
                    return null;

                if (competenceSummary is null)
                    return null;

                return new CompetenceResultHistoryModel
                {
                    AssessmentId = assessment.Id,
                    AssessmentDate = assessment.EndsAt,
                    CompetenceScore = competenceSummary.AverageScore
                };
            })
            .Where(h => h is not null)
            .Cast<CompetenceResultHistoryModel>()
            .OrderBy(h => h.AssessmentDate)
            .ToList();

        return history;
    }
}