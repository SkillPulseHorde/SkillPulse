using AssessmentService.Application.ServiceClientsAbstract;
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
    
    public required List<Guid> EvaluatorIds { get; init; }
}

public sealed class CreateAssessmentCommandHandler(
    IAssessmentRepository assessmentRepository,
    IUserServiceClient userServiceClient,
    IUserEvaluatorRepository userEvaluatorRepository)
    : IRequestHandler<CreateAssessmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAssessmentCommand request, CancellationToken ct)
    {
        var allUserIdsToCheck = request.EvaluatorIds
            .Append(request.CreatedByUserId)
            .Append(request.EvaluateeId)
            .ToList();
        
        var areUsersExist = await userServiceClient.AreUsersExistAsync(allUserIdsToCheck, ct);
        if (!areUsersExist)
            return Error.NotFound("Заданные пользователи не существуют");
        
        
        var hasOverlapping = await assessmentRepository.HasOverlappingAssessmentAsync(
            request.EvaluateeId, 
            request.StartAt, 
            request.EndsAt, 
            ct);
        
        if (hasOverlapping)
            return Error.Conflict("В заданном периоде уже существует аттестация для этого пользователя");

        var assessment = new Assessment
        {
            EvaluateeId = request.EvaluateeId,
            StartAt = request.StartAt,
            EndsAt = request.EndsAt,
            CreatedByUserId = request.CreatedByUserId,
            Evaluators = request.EvaluatorIds.Distinct()
                .Select(evaluatorId => new AssessmentEvaluator
                {
                    EvaluatorId = evaluatorId
                })
                .ToList()
        };
        
        var createdAssessmentId = await assessmentRepository.CreateAsync(assessment, ct);
        
        // Обновляем список оценщиков глобально для пользователя
        await userEvaluatorRepository.UpdateEvaluatorsForUserAsync(request.EvaluateeId, request.EvaluatorIds.Distinct().ToList(), ct);
        
        return createdAssessmentId;
    }
}

public class CreateAssessmentCommandValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentCommandValidator()
    {
        RuleFor(x => x.EvaluateeId).NotEmpty();
        RuleFor(x => x.EvaluatorIds).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndsAt)
            .WithMessage("StartAt должно быть раньше, чем EndsAt");
        RuleFor(x => x.StartAt).GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("StartAt не может быть раньше текущей даты");
    }
}
