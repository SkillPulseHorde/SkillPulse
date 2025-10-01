namespace UserService.Domain.Entities;

public record class User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? MidName { get; init; }
    
    public Guid? ManagerId { get; init; }
    public User? Manager { get; init; }
    public List<User> Subordinates { get; init; } = [];
    public required Position Position { get; init; } = Position.NotDefined;
}