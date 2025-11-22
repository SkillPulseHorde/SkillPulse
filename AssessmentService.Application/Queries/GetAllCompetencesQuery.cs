using Common;
using MediatR;
using AssessmentService.Application.Models;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Extensions;
using AssessmentService.Domain.Repos;

namespace AssessmentService.Application.Queries;

public sealed record GetAllCompetencesQuery(Guid EvaluateeId) : IRequest<Result<List<CompetenceModel>>>;

public sealed class GetAllCompetencesQueryHandler(
    ICompetenceRepository competenceRepository,
    IUserServiceClient userServiceClient)
    : IRequestHandler<GetAllCompetencesQuery, Result<List<CompetenceModel>>>
{
    public async Task<Result<List<CompetenceModel>>> Handle(GetAllCompetencesQuery request, CancellationToken ct)
    {
        var competences = await competenceRepository.GetAllCompetencesReadOnlyAsync(ct);
        var userInfo = await userServiceClient.GetUsersByIdsAsync([request.EvaluateeId], ct);
        if (userInfo.Count == 0)
            return Error.NotFound($"Пользователь с ID {request.EvaluateeId} не найден");

        
        var userGrade = userInfo.First().Grade;

        var competenceModels = competences
            .Select(c => new CompetenceModel
            {
                Id = c.Id,
                Name = c.Name,
                Criteria = c.Criteria.Select(cr => new CriterionModel
                {
                    Id = cr.Id,
                    Name = cr.Name,
                    IsMandatory = cr.IsMandatoryCriterion(userGrade)
                }).ToList()
            })
            .ToList();

        return competenceModels;
    }
}
