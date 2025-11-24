using Common;
using MediatR;
using AssessmentService.Application.Models;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Repos;

namespace AssessmentService.Application.Queries;

public sealed record GetActiveAssessmentsByEvaluatorIdQuery(Guid EvaluatorId) : IRequest<Result<List<AssessmentModel>>>;

public sealed class GetActiveAssessmentsByEvaluatorIdQueryHandler(
    IAssessmentRepository assessmentRepository,
    IUserServiceClient userServiceClient)
    : IRequestHandler<GetActiveAssessmentsByEvaluatorIdQuery, Result<List<AssessmentModel>>>
{
    public async Task<Result<List<AssessmentModel>>> Handle(GetActiveAssessmentsByEvaluatorIdQuery request, CancellationToken ct)
    {
        var isEvaluatorExists = await userServiceClient.AreUsersExistAsync([request.EvaluatorId], ct);
        if (!isEvaluatorExists)
            return Error.NotFound($"Пользователь {request.EvaluatorId} не найден.");
        
        var assessments = await assessmentRepository.GetActiveAssessmentsByEvaluatorIdReadonlyAsync(request.EvaluatorId, ct);
        
        if (assessments.Count == 0)
            return Result<List<AssessmentModel>>.Success([]);

        
        var evaluateeIds = assessments.Select(a => a.EvaluateeId).Distinct().ToList();
        var users = await userServiceClient.GetUsersByIdsAsync(evaluateeIds, ct);
        var userDict = users.ToDictionary(u => u.Id, u => u);

        var assessmentModels = assessments
            .Select(a =>
            {
                var user = userDict.GetValueOrDefault(a.EvaluateeId);
                if (user == null)
                {
                    return null; // Не включаем аттестацию, если пользователь не найден
                }
                
                return new AssessmentModel
                {
                    Id = a.Id,
                    EvaluateeId = a.EvaluateeId,
                    EvaluateeFullName = user.FullName,
                    EvaluateePosition = user.Position,
                    EvaluateeTeamName = user.TeamName,
                    StartAt = a.StartAt,
                    EndsAt = a.EndsAt,
                };
            })
            .OfType<AssessmentModel>()
            .ToList();
        
        return assessmentModels;
    }
}
