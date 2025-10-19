namespace AssessmentService.Application.Models;

public record CriterionEvaluationModel(
    Guid Id,
    Guid CriterionId,
    Guid CompetenceEvaluationId,
    int? Score,
    string? Comment);

public static class CriterionEvaluationModelExtensions
{
    public static CriterionEvaluationModel ToAppModel(this Domain.Entities.CriterionEvaluation criterionEvaluation) =>
        new CriterionEvaluationModel(
            criterionEvaluation.Id,
            criterionEvaluation.CriterionId,
            criterionEvaluation.CompetenceEvaluationId,
            criterionEvaluation.Score,
            criterionEvaluation.Comment
        );
}
        
    
        