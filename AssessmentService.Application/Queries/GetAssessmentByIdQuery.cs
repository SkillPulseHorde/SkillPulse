using AssessmentService.Application.Models;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Repos;
using Common;
using MediatR;

namespace AssessmentService.Application.Queries;

public sealed record GetAssessmentByIdQuery(Guid AssessmentId) : IRequest<Result<AssessmentDetailModel>>;

public sealed class GetAssessmentByIdQueryHandler(
    IAssessmentRepository assessmentRepository,
    IUserServiceClient userServiceClient)
    : IRequestHandler<GetAssessmentByIdQuery, Result<AssessmentDetailModel>>
{
    public async Task<Result<AssessmentDetailModel>> Handle(GetAssessmentByIdQuery request, CancellationToken ct)
    {
        var assessment = await assessmentRepository.GetByIdReadonlyAsync(request.AssessmentId, ct);

        if (assessment is null)
        {
            return Error.NotFound($"Аттестация с ID {request.AssessmentId} не найдена");
        }

        var evaluatorIds = assessment.Evaluators.Select(e => e.EvaluatorId).ToList();
        var allUserIds = evaluatorIds
            .Append(assessment.EvaluateeId)
            .Distinct()
            .ToList();

        var users = await userServiceClient.GetUsersByIdsAsync(allUserIds, ct);
        var userDict = users.ToDictionary(u => u.Id, u => u);

        var evaluatee = userDict.GetValueOrDefault(assessment.EvaluateeId);
        if (evaluatee == null)
            return Error.NotFound($"Оцениваемый пользователь с ID {assessment.EvaluateeId} не найден");


        var model = new AssessmentDetailModel
        {
            Id = assessment.Id,
            EvaluateeId = assessment.EvaluateeId,
            EvaluateeFullName = evaluatee.FullName,
            EvaluateePosition = evaluatee.Position,
            EvaluateeTeamName = evaluatee.TeamName,
            StartAt = assessment.StartAt,
            EndsAt = assessment.EndsAt,
            EvaluatorIds = evaluatorIds
        };

        return model;
    }
}