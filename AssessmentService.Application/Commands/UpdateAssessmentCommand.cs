using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using Common;
using MediatR;
using FluentValidation;

namespace AssessmentService.Application.Commands;

public sealed record UpdateAssessmentCommand : IRequest<Result<Guid>>
{
    public required Guid AssessmentId { get; init; }
    
    public required DateTime EndsAt { get; init; }
    
    public required List<Guid> EvaluatorIds { get; init; }
}

public sealed class UpdateAssessmentCommandHandler(
    IAssessmentRepository assessmentRepository,
    IUserServiceClient userServiceClient)
    : IRequestHandler<UpdateAssessmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateAssessmentCommand request, CancellationToken ct)
    {
        // Проверяем существование всех оценщиков
        var areUsersExist = await userServiceClient.UsersExistAsync(request.EvaluatorIds, ct);
        if (!areUsersExist)
        {
            return Result<Guid>.Failure(Error.NotFound("Один или несколько оценщиков не существуют"));
        }

        var assessment = await assessmentRepository.GetByIdReadonlyAsync(request.AssessmentId, ct);
        if (assessment == null)
        {
            return Result<Guid>.Failure(Error.NotFound("Аттестация не найдена"));
        }

        if (assessment.EndsAt < DateTime.UtcNow)
        {
            return Result<Guid>.Failure(Error.Validation("Нельзя изменить завершенную аттестацию"));
        }

        assessment.EndsAt = request.EndsAt;
        
        // Проверяем, не началась ли аттестация, если пытаемся изменить список оценщиков
        if (assessment.StartAt <= DateTime.UtcNow)
        {
            // Проверяем, изменился ли список оценщиков
            var existingEvaluatorIds = assessment.Evaluators.Select(e => e.EvaluatorId).OrderBy(id => id).ToList();
            var newEvaluatorIds = request.EvaluatorIds.Distinct().OrderBy(id => id).ToList();
            
            if (!existingEvaluatorIds.SequenceEqual(newEvaluatorIds))
            {
                return Result<Guid>.Failure(Error.Validation("Нельзя изменить список оценщиков после начала аттестации"));
            }
        }
        else
        {
            // Обновляем список оценщиков только если аттестация не началась
            assessment.Evaluators.Clear();
            foreach (var evaluatorId in request.EvaluatorIds.Distinct())
            {
                assessment.Evaluators.Add(new AssessmentEvaluator
                {
                    AssessmentId = assessment.Id,
                    EvaluatorId = evaluatorId
                });
            }
        }

        await assessmentRepository.UpdateAsync(assessment, ct);

        return Result<Guid>.Success(assessment.Id);
    }
}

public class UpdateAssessmentCommandValidator : AbstractValidator<UpdateAssessmentCommand>
{
    public UpdateAssessmentCommandValidator()
    {
        RuleFor(x => x.AssessmentId).NotEmpty();
        RuleFor(x => x.EvaluatorIds).NotEmpty();
        RuleFor(x => x.EndsAt)
            .Must(e => e.ToUniversalTime().Date >= DateTime.UtcNow.Date)
            .WithMessage("Дата завершения должна быть не раньше сегодняшней");
    }
}