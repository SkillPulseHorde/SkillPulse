namespace AssessmentService.Application.Models;

public record CompetenceEvaluationModel(
    Guid Id,
    Guid CompetenceId,
    Guid EvaluationId,
    string Comment);

public static class CompetenceEvaluationModelExtensions
{
    public static CompetenceEvaluationModel ToAppModel(this Domain.Entities.CompetenceEvaluation competenceEvaluation) =>
        new CompetenceEvaluationModel(
            competenceEvaluation.Id,
            competenceEvaluation.CompetenceId,
            competenceEvaluation.EvaluationId,
            competenceEvaluation.Comment
        );
}
    
