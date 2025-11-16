using Common;
using MediatR;
using AssessmentService.Application.Models;
using AssessmentService.Domain.Repos;

namespace AssessmentService.Application.Queries;

public sealed record GetAllCompetencesQuery : IRequest<Result<List<CompetenceModel>>>;

public sealed class GetAllCompetencesQueryHandler(ICompetenceRepository competenceRepository)
    : IRequestHandler<GetAllCompetencesQuery, Result<List<CompetenceModel>>>
{
    public async Task<Result<List<CompetenceModel>>> Handle(GetAllCompetencesQuery request, CancellationToken ct)
    {
        var competences = await competenceRepository.GetAllCompetencesReadOnlyAsync(ct);

        var competenceModels = competences
            .Select(c => new CompetenceModel
            {
                Id = c.Id,
                Name = c.Name,
                Criteria = c.Criteria.Select(cr => new CriterionModel
                {
                    Id = cr.Id,
                    Name = cr.Name
                }).ToList()
            })
            .ToList();

        return competenceModels;
    }
}

