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
    IUserServiceClient userServiceClient,
    IUserEvaluatorRepository userEvaluatorRepository)
    : IRequestHandler<UpdateAssessmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateAssessmentCommand request, CancellationToken ct)
    {
        // Проверяем существование всех оценщиков
        var areUsersExist = await userServiceClient.AreUsersExistAsync(request.EvaluatorIds, ct);
        if (!areUsersExist)
            return Error.NotFound("Один или несколько оценщиков не существуют");

        var assessment = await assessmentRepository.GetByIdAsync(request.AssessmentId, ct);
        if (assessment == null)
            return Error.NotFound("Аттестация не найдена");

        if (assessment.EndsAt < DateTime.UtcNow)
            return Error.Validation("Нельзя изменить завершенную аттестацию");
        if (request.EndsAt <= assessment.StartAt)
            return Error.Validation("Дата завершения должна быть позже даты начала аттестации");

        assessment.EndsAt = request.EndsAt;

        // Проверяем, не началась ли аттестация, если пытаемся изменить список оценщиков
        if (assessment.StartAt <= DateTime.UtcNow)
        {
            // Проверяем, изменился ли список оценщиков
            var existingEvaluatorIds = assessment.Evaluators.Select(e => e.EvaluatorId).OrderBy(id => id).ToList();
            var newEvaluatorIds = request.EvaluatorIds.Distinct().OrderBy(id => id).ToList();

            if (!existingEvaluatorIds.SequenceEqual(newEvaluatorIds))
                return Error.Validation("Нельзя изменить список оценщиков после начала аттестации");
        }
        else // Обновляем список оценщиков только если аттестация не началась
        {
            if (request.EvaluatorIds.Contains(assessment.EvaluateeId))
                return Error.Validation("Оцениваемый пользователь не может быть оценщиком в своей аттестации");

            assessment.Evaluators.Clear();
            foreach (var evaluatorId in request.EvaluatorIds.Distinct())
            {
                assessment.Evaluators.Add(new AssessmentEvaluator
                {
                    AssessmentId = assessment.Id,
                    EvaluatorId = evaluatorId
                });
            }

            // Обновляем список оценщиков глобально для пользователя
            await userEvaluatorRepository.UpdateEvaluatorsForUserAsync(
                assessment.EvaluateeId,
                request.EvaluatorIds.Distinct().ToList(),
                ct);
        }

        await assessmentRepository.UpdateAsync(assessment, ct);

        return assessment.Id;
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