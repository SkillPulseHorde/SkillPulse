using UserService.Application.Models;
using UserService.Domain.Entities;

namespace UserService.Dto;

public sealed record UserShortInfoDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MidName,
    string TeamName,
    Position Position,
    Grade Grade);

public static class UserShortInfoDtoExtensions
{
    public static UserShortInfoDto ToDto(this User user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.MidName,
            user.TeamName ?? string.Empty,
            user.Position,
            user.Grade);

    public static UserShortInfoDto ToDto(this UserModel userModel) =>
        new(
            userModel.Id,
            userModel.FirstName,
            userModel.LastName,
            userModel.MidName,
            userModel.TeamName ?? string.Empty,
            userModel.Position,
            userModel.Grade);
}