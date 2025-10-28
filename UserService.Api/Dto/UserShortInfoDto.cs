using UserService.Application.Models;
using UserService.Domain.Entities;

namespace UserService.Dto;

public sealed record UserShortInfoDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MidName,
    string TeamName,
    Position Position);

public static class UserShortInfoDtoExtensions
{
    public static UserShortInfoDto ToDto(this User user) =>
        new UserShortInfoDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.MidName,
            user.TeamName ?? string.Empty,
            user.Position);
}