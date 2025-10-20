namespace UserService.Domain.Entities;

public record User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? MidName { get; init; }
    
    public string? Email { get; init; }
    
    public required Grade Grade { get; init; }
    
    public string? TeamName { get; init; }
    
    public Guid? ManagerId { get; init; }
    public User? Manager { get; init; }
    public List<User> Subordinates { get; init; } = [];
    public required Position Position { get; init; } = Position.NotDefined;
}