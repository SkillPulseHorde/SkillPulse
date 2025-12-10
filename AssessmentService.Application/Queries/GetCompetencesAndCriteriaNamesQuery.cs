using AssessmentService.Application.Models;
using Common;
using MediatR;
using AssessmentService.Domain.Repos;

namespace AssessmentService.Application.Queries;

public sealed record GetCompetencesAndCriteriaNamesQuery : IRequest<Result<CompetencesAndCriteriaNamesModel>>;

public sealed class GetCompetencesAndCriteriaNamesQueryHandler(
    ICompetenceRepository competenceRepository)
    : IRequestHandler<GetCompetencesAndCriteriaNamesQuery, Result<CompetencesAndCriteriaNamesModel>>
{
    public async Task<Result<CompetencesAndCriteriaNamesModel>> Handle(GetCompetencesAndCriteriaNamesQuery request, CancellationToken ct)
    {
        var competences = await competenceRepository.GetAllCompetencesReadOnlyAsync(ct);
        if (competences.Length == 0)
            return Error.NotFound("Компетенции не найдены");

        var competenceNames = competences.ToDictionary(c => c.Id, c => c.Name);
        
        var criterionNames = competences
            .SelectMany(c => c.Criteria)
            .ToDictionary(cr => cr.Id, cr => cr.Name);

        return new CompetencesAndCriteriaNamesModel
        {
            CompetenceNames = competenceNames,
            CriterionNames = criterionNames
        };
    }
}

