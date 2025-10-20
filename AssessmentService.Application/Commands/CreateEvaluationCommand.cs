using MediatR;

namespace AssessmentService.Application.Commands;

public sealed record CreateEvaluationCommand : IRequest<Guid>
{
    public required Guid EvaluatorId { get; init; }
    
    public required Guid AssessmentId { get; init; }
}

