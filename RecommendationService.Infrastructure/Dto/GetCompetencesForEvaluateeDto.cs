using RecommendationService.Application.Models;

namespace RecommendationService.Infrastructure.Dto;

public sealed record GetCompetencesForEvaluateeDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required List<CriterionResponseDto> Criteria { get; init; } = [];

    public CompetenceModel ToModel()
    {
        return new CompetenceModel()
        {
            Id = Id,
            Name = Name,
            Criteria = Criteria.Select(a => new CriterionModel
            {
                Id = a.Id,
                Name = a.Name,
                IsMandatory = a.IsMandatory
            }).ToList()
        };
    }
}

public sealed record CriterionResponseDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required bool IsMandatory { get; set; }
}