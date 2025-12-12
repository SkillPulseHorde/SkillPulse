using AssessmentService.Application.Models;

namespace AssessmentService.Api.Dto;

public class CompetenceResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<CriterionResponseDto> Criteria { get; set; } = [];
}

public class CriterionResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required bool IsMandatory { get; set; }
}

public static class CompetenceResponseDtoExtensions
{
    public static CompetenceResponseDto ToDto(this CompetenceModel competence)
    {
        return new CompetenceResponseDto
        {
            Id = competence.Id,
            Name = competence.Name,
            Description = competence.Description,
            Criteria = competence.Criteria.Select(c => new CriterionResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsMandatory = c.IsMandatory
            }).ToList()
        };
    }
}
