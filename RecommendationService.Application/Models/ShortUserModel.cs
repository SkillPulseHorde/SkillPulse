namespace RecommendationService.Application.Models;

public record ShortUserModel
{
    public Guid Id { get; set; }

    public string Position { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;
}