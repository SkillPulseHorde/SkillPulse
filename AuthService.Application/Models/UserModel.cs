namespace AuthService.Application.Models;

public record UserModel(
    string FirstName,
    string LastName,
    string? MidName,
    string? Email,
    string? Grade,
    string? TeamName,
    string? ManagerName,
    string Position
);