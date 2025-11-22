using AssessmentService.Application.Models;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Repos;
using Common;
using MediatR;

namespace AssessmentService.Application.Queries;

public sealed record GetCompletedAssessmentsByEvaluateeIdQuery(Guid EvaluateeId)
    : IRequest<Result<List<AssessmentShortInfoModel>>>;

public sealed class GetCompletedAssessmentsByEvaluateeIdQueryHandler(
    IAssessmentRepository assessmentRepository,
    IUserServiceClient userServiceClient)
    : IRequestHandler<GetCompletedAssessmentsByEvaluateeIdQuery, Result<List<AssessmentShortInfoModel>>>
{
    public async Task<Result<List<AssessmentShortInfoModel>>> Handle(GetCompletedAssessmentsByEvaluateeIdQuery request,
        CancellationToken ct)
    {
        var isEvaluateeExists = await userServiceClient.AreUsersExistAsync([request.EvaluateeId], ct);
        if (!isEvaluateeExists)
            return Error.NotFound($"Пользователь {request.EvaluateeId} не найден.");

        var assessments = await assessmentRepository
            .GetCompletedAssessmentsByEvaluateeIdReadonlyAsync(request.EvaluateeId, ct);

        if (assessments.Count == 0)
            return new List<AssessmentShortInfoModel>();
        
        var models = assessments
            .Select(a => new AssessmentShortInfoModel()
            {
                Id = a.Id,
                StartAt = a.StartAt,
                EndsAt = a.EndsAt,
            })
            .ToList();

        return models;
    }
}
