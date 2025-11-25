using AssessmentService.Domain;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Domain.ValueObjects;

namespace AssessmentService.Application;

public sealed class EvaluationAnalyzer(
    IAssessmentResultRepository assessmentResultRepository,
    IEvaluationRepository evaluationRepository,
    ICompetenceRepository competenceRepository)
    : IEvaluationAnalyzer
{
    public async Task<AssessmentResult?> GetAssessmentResultByAssessmentIdAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await assessmentResultRepository.GetByAssessmentIdAsync(assessmentId, ct) ??
               await CreateAndGetAssessmentResultAsync(assessmentId, ct);
    }
    
    public async Task<List<AssessmentResult>> CreateAssessmentResultsAsync(List<Guid> assessmentIds, CancellationToken ct = default)
    {
        if (assessmentIds.Count == 0)
            return [];

        // Получаем все оценки для всех assessmentId
        var evaluationsByAssessmentId = await evaluationRepository.GetEvaluationsByAssessmentIdsReadonlyAsync(assessmentIds, ct);
        
        var competenceCriteriaMap = await competenceRepository.GetCompetenceCriteriaMapAsync(ct);
        
        var resultsToCreate = new List<AssessmentResult>();

        foreach (var assessmentId in assessmentIds)
        {
            // Если для этой оценки нет evaluations, пропускаем
            if (!evaluationsByAssessmentId.TryGetValue(assessmentId, out var evaluations) || evaluations.Length == 0)
                continue;

            var assessmentResult = CalculateAssessmentResult(assessmentId, evaluations, competenceCriteriaMap);
            
            if (assessmentResult is not null)
                resultsToCreate.Add(assessmentResult);
        }

        // Сохраняем все результаты
        if (resultsToCreate.Count > 0)
            await assessmentResultRepository.CreateRangeAsync(resultsToCreate, ct);

        return resultsToCreate;
    }
    
    private async Task<AssessmentResult?> CreateAndGetAssessmentResultAsync(Guid assessmentId, CancellationToken ct = default)
    {
        var evaluations = await evaluationRepository.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, ct);
        if (evaluations.Length == 0)
            return null; // Если никто не оценил
        
        var competenceCriteriaMap = await competenceRepository.GetCompetenceCriteriaMapAsync(ct);
        
        var assessmentResult = CalculateAssessmentResult(assessmentId, evaluations, competenceCriteriaMap);
        
        if (assessmentResult is not null)
            await assessmentResultRepository.CreateAsync(assessmentResult, ct);
        
        return assessmentResult;
    }

    private AssessmentResult? CalculateAssessmentResult(
        Guid assessmentId, 
        Evaluation[] evaluations, 
        Dictionary<Guid, List<Guid>> competenceCriteriaMap)
    {
        var competenceSummaries = competenceCriteriaMap.Keys.ToDictionary(
            competenceId => competenceId,
            competenceId =>
            {
                // Собираем все оценки для данной компетенции от всех оценщиков
                var competenceEvaluationsForCompetence = evaluations
                    .Where(e => e.CompetenceEvaluations != null)
                    .SelectMany(e => e.CompetenceEvaluations!
                        .Where(ce => ce.CompetenceId == competenceId && ce.CriterionEvaluations != null) // не включаем исключенные из-за отсутствия (обязательных критериев) компетенции
                        .Select(ce => new { Evaluation = e, CompetenceEvaluation = ce }))
                    .ToList();

                if (competenceEvaluationsForCompetence.Count == 0)
                    return null;
                
                // Собираем комментарии к компетенции
                var competenceComments = competenceEvaluationsForCompetence
                    .Where(x => !string.IsNullOrWhiteSpace(x.CompetenceEvaluation.Comment))
                    .Select(x => x.CompetenceEvaluation.Comment)
                    .ToList();
                
                // Получаем критерии для данной компетенции
                var allCriterionIds = competenceCriteriaMap[competenceId];
                
                var criterionSummaries = allCriterionIds
                    .Select(criterionId =>
                    {
                        // Собираем все оценки для данного критерия от всех оценщиков
                        var criterionEvals = competenceEvaluationsForCompetence
                            .Select(item => new
                            {
                                CriterionEval = item.CompetenceEvaluation.CriterionEvaluations?
                                    .FirstOrDefault(cre => cre.CriterionId == criterionId),
                                item.Evaluation.RoleRatio
                            })
                            .Where(x => x.CriterionEval is { Score: not null }) // Не включаем "не могу оценить" результаты
                            .ToList();

                        if (criterionEvals.Count == 0)
                            return null;
                        
                        var totalWeightedScore = criterionEvals
                            .Sum(x => x.CriterionEval!.Score!.Value * x.RoleRatio);
                        
                        var totalWeight = criterionEvals
                            .Sum(x => x.RoleRatio);
                        
                        var criterionComments = criterionEvals
                            .Where(x => !string.IsNullOrWhiteSpace(x.CriterionEval!.Comment))
                            .Select(x => x.CriterionEval!.Comment!)
                            .ToList();
                        
                        return new
                        {
                            CriterionId = criterionId,
                            TotalWeightedScore = totalWeightedScore,
                            TotalWeight = totalWeight,
                            AverageScore = totalWeight > 0 
                                ? Math.Round((double)totalWeightedScore / totalWeight, 2) 
                                : 0,
                            Comments = criterionComments
                        };
                    })
                    .Where(x => x is not null && x.TotalWeight > 0)
                    .ToList();
                
                if (criterionSummaries.Count == 0)
                    return null;

                var competenceAverageScore = Math.Round(criterionSummaries.Average(x => x!.AverageScore), 2);
                    
                var criterionSummariesDict = criterionSummaries.ToDictionary(
                    x => x!.CriterionId,
                    x => new CriterionSummary(x!.AverageScore, x.Comments));
                
                return new CompetenceSummary(
                    competenceAverageScore,
                    criterionSummariesDict,
                    competenceComments);
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        if (competenceSummaries.Values.All(cs => cs is null))
            return null;
        
        return new AssessmentResult
        {
            AssessmentId = assessmentId,
            Data = new AssessmentResultData(competenceSummaries)
        };
    }
}