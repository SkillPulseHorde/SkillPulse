using UserService.Domain.Entities;

namespace UserService.Application.Models;

public record UserModel(
    Guid Id,
    string FirstName,
    string LastName,
    string? MidName,
    string? Email,
    Grade Grade,
    string? TeamName,
    string? ManagerName,
    Guid? ManagerId,
    Position Position);

public static class UserModelExtensions
{
    public static UserModel ToAppModel(this User user) =>
        new UserModel(
            user.Id,
            user.FirstName,
            user.LastName,
            user.MidName,
            user.Email,
            user.Grade,
            user.TeamName,
            user.Manager == null
                ? null
                : $"{user.Manager.LastName} {user.Manager.MidName} {user.Manager.FirstName}",
            user.ManagerId,
            user.Position);
}