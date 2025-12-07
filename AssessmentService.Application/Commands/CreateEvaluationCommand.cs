using AssessmentService.Application.Commands.CommandParameters;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using Common;
using Common.Shared.Auth.Constants;
using MediatR;
using FluentValidation;

namespace AssessmentService.Application.Commands;

public sealed record CreateEvaluationCommand : IRequest<Result<Guid>>
{
    public required Guid AssessmentId { get; init; }

    public required Guid EvaluatorId { get; init; }

    public required string EvaluatorRole { get; init; }

    public required List<CompetenceEvaluationCommandParameter> CompetenceEvaluations { get; init; }
}

public sealed class CreateEvaluationCommandHandler(
    IEvaluationRepository evaluationRepository,
    IAssessmentRepository assessmentRepository,
    ICompetenceRepository competenceRepository)
    : IRequestHandler<CreateEvaluationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateEvaluationCommand request, CancellationToken ct)
    {
        var assessment = await assessmentRepository.GetByIdReadonlyAsync(request.AssessmentId, ct);
        if (assessment == null)
            return Error.NotFound($"Аттестация с ID {request.AssessmentId} не найдена");
        
        // Проверяем, что аттестация активна
        var now = DateTime.UtcNow;
        if (now < assessment.StartAt || now > assessment.EndsAt)
            return Error.Conflict("Аттестация не активна в данный момент");
        
        // Проверяем, что оценщик назначен на эту аттестацию
        var isCurrentEvaluatorAssigned = assessment.Evaluators.Any(e => e.EvaluatorId == request.EvaluatorId);
        if (!isCurrentEvaluatorAssigned)
            return Error.Forbidden("Оценщик не назначен на данную аттестацию");
        
        // Проверяем существование всех компетенций
        var competenceIds = request.CompetenceEvaluations.Select(ce => ce.CompetenceId).ToList();
        var existingCompetences = await competenceRepository.GetByIdsAsync(competenceIds, ct);
        if (existingCompetences.Count != competenceIds.Count)
        {
            return Error.NotFound("Не все компетенции найдены");
        }

        // Проверяем, что все критерии принадлежат соответствующим компетенциям
        foreach (var compEval in request.CompetenceEvaluations
                     .Where(ce => ce.CriterionEvaluations != null))
        {
            var competence = existingCompetences.FirstOrDefault(c => c.Id == compEval.CompetenceId);
            if (competence == null) continue;

            var criterionIds = compEval.CriterionEvaluations!.Select(ce => ce.CriterionId).ToList();
            var validCriterionIds = competence.Criteria.Select(cr => cr.Id).ToHashSet();

            if (criterionIds.Any(cid => !validCriterionIds.Contains(cid)))
                return Error.Conflict($"Не все критерии в компетенции '{competence.Name}' принадлежат ей");
        }

        var roleRatio = Roles.AllManagersAndHr.Contains(request.EvaluatorRole)
            ? Evaluation.ManagerScoreRatio
            : Evaluation.DefaultScoreRatio;

        var evaluationId = Guid.NewGuid();
        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EvaluatorId = request.EvaluatorId,
            RoleRatio = roleRatio,
            AssessmentId = request.AssessmentId,
            SubmittedAt = DateTime.UtcNow,
            CompetenceEvaluations = request.CompetenceEvaluations.Select(ce =>
            {
                var competenceEvaluationId = Guid.NewGuid();
                return new CompetenceEvaluation
                {
                    Id = competenceEvaluationId,
                    CompetenceId = ce.CompetenceId,
                    EvaluationId = evaluationId,
                    Comment = ce.CompetenceComment ?? string.Empty,
                    CriterionEvaluations = ce.CriterionEvaluations?.Select(cre => new CriterionEvaluation
                    {
                        Id = Guid.NewGuid(),
                        CriterionId = cre.CriterionId,
                        CompetenceEvaluationId = competenceEvaluationId,
                        Score = cre.Score,
                        Comment = cre.CriterionComment
                    }).ToList()
                };
            }).ToList()
        };

        var createdEvaluationId = await evaluationRepository.CreateAsync(evaluation, ct);

        return createdEvaluationId;
    }
}

public class CreateEvaluationCommandValidator : AbstractValidator<CreateEvaluationCommand>
{
    public CreateEvaluationCommandValidator()
    {
        RuleFor(x => x.AssessmentId).NotEmpty();
        RuleFor(x => x.EvaluatorId).NotEmpty();
        RuleFor(x => x.CompetenceEvaluations)
            .NotEmpty().WithMessage("Список оценок компетенций не должен быть пустым")
            .Must(ce => ce.Select(c => c.CompetenceId).Distinct().Count() == ce.Count)
            .WithMessage("Компетенции не должны повторяться");

        RuleForEach(x => x.CompetenceEvaluations).ChildRules(comp =>
        {
            comp.RuleFor(c => c.CompetenceId).NotEmpty();

            comp.When(c => c.CriterionEvaluations != null, () =>
            {
                comp.RuleFor(c => c.CompetenceComment)
                    .NotEmpty()
                    .WithMessage("Комментарий к компетенции обязателен");

                comp.RuleFor(c => c.CriterionEvaluations)
                    .NotEmpty().WithMessage("Список оценок критериев не должен быть пустым")
                    .Must(cre => cre!.Select(cr => cr.CriterionId).Distinct().Count() == cre!.Count)
                    .WithMessage("Критерии не должны повторяться");

                comp.RuleForEach(c => c.CriterionEvaluations).ChildRules(crit =>
                {
                    crit.RuleFor(cr => cr.CriterionId).NotEmpty();
                    crit.RuleFor(cr => cr.Score)
                        .InclusiveBetween(CriterionEvaluation.MinScore, CriterionEvaluation.MaxScore)
                        .When(cr => cr.Score.HasValue)
                        .WithMessage($"Оценка должна быть от {CriterionEvaluation.MinScore} до {CriterionEvaluation.MaxScore}");
                });
            });
        });
    }
}
