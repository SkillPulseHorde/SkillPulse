using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using Common;
using MediatR;
using FluentValidation;

namespace AssessmentService.Application.Commands;

public sealed record CreateAssessmentCommand : IRequest<Result<Guid>>
{
    public required Guid EvaluateeId { get; init; }
    
    public required DateTime StartAt { get; init; }
    
    public required DateTime EndsAt { get; init; }
    
    public required Guid CreatedByUserId { get; init; }
}

public sealed class CreateAssessmentCommandHandler(
    IAssessmentRepository assessmentRepository)
    : IRequestHandler<CreateAssessmentCommand, Result<Guid>>
{
    private readonly CreateAssessmentCommandValidator _createAssessmentCommandValidator = new();
    
    public async Task<Result<Guid>> Handle(CreateAssessmentCommand request, CancellationToken ct)
    {
        await _createAssessmentCommandValidator.ValidateAndThrowAsync(request, cancellationToken: ct);
        
        var assessment = new Assessment
        {
            EvaluateeId = request.EvaluateeId,
            StartAt = request.StartAt,
            EndsAt = request.EndsAt,
            CreatedByUserId = request.CreatedByUserId,
        };
        
        var createdAssessmentId = await assessmentRepository.CreateAsync(assessment, ct);
        
        return Result<Guid>.Success(createdAssessmentId);
    }
}

public class CreateAssessmentCommandValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentCommandValidator()
    {
        RuleFor(x => x.EvaluateeId).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndsAt).WithMessage("StartAt должно быть раньше, чем  EndsAt");
    }
}
