namespace AssessmentService.Api.Dto.Evaluation;

public sealed record CreateEvaluationRequestDto
{
    public required Guid AssessmentId { get; init; }

    public required Guid EvaluatorId { get; init; }

    public required List<CompetenceEvaluationDto> CompetenceEvaluations { get; init; }
}