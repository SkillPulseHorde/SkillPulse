using Common;
using MediatR;
using AssessmentService.Application.Models;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Repos;

namespace AssessmentService.Application.Queries;

public sealed record GetAssessmentsQuery(bool IsActive) : IRequest<Result<List<AssessmentModel>>>;

public sealed class GetAssessmentsQueryHandler(
    IAssessmentRepository assessmentRepository,
    IUserServiceClient userServiceClient)
    : IRequestHandler<GetAssessmentsQuery, Result<List<AssessmentModel>>>
{
    public async Task<Result<List<AssessmentModel>>> Handle(GetAssessmentsQuery request, CancellationToken ct)
    {
        var assessments = await assessmentRepository.GetAssessmentsReadonlyAsync(request.IsActive, ct);
        
        var evaluateeIds = assessments.Select(a => a.EvaluateeId).Distinct().ToList();
        var users = await userServiceClient.GetUsersByIdsAsync(evaluateeIds, ct);
        var userDict = users.ToDictionary(u => u.Id, u => u);

        var assessmentModels = assessments.Select(a =>
        {
            var user = userDict.GetValueOrDefault(a.EvaluateeId);
            return new AssessmentModel
            {
                Id = a.Id,
                EvaluateeId = a.EvaluateeId,
                EvaluateeFullName = user?.FullName,
                EvaluateePosition = user?.Position,
                EvaluateeTeamName = user?.TeamName,
                StartAt = a.StartAt,
                EndsAt = a.EndsAt,
            };
        }).ToList();
        
        return Result<List<AssessmentModel>>.Success(assessmentModels);
    }
}