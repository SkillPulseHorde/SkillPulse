namespace RecommendationService.Application.Models;

public sealed record ShortUserModel
{
    public Guid Id { get; set; }

    public string Position { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;

    public string TeamName { get; set; } = string.Empty;

    public Guid? ManagerId { get; set; }
}