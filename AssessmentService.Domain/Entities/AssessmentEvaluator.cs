namespace AssessmentService.Domain.Entities;

public sealed class AssessmentEvaluator
{
    public Guid AssessmentId { get; init; }
    public Guid EvaluatorId { get; init; }


    public Assessment Assessment { get; init; } = null!;
}