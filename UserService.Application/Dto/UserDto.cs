using UserService.Domain.Entities;

namespace UserService.Application.Dto;

public record UserDto(
    string FirstName,
    string LastName,
    string? MidName,
    string? ManagerName,
    Position Position);

public static class UserDtoExtensions
{
    public static UserDto ToDto(this User user) =>
        new UserDto(
            user.FirstName,
            user.LastName,
            user.MidName,
            user.Manager == null
            ? null
            : $"{user.Manager.LastName} {user.Manager.MidName} {user.Manager.FirstName}",
            user.Position);
}