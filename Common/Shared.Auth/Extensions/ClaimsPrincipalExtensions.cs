using System.Security.Claims;

namespace Common.Shared.Auth.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
            throw new UnauthorizedAccessException("User ID не найден в токене");

        return !Guid.TryParse(userIdString, out var userId)
            ? throw new InvalidOperationException("Неправильный формат User ID в токене")
            : userId;
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        var role = user.FindFirstValue(ClaimsIdentity.DefaultRoleClaimType);

        return string.IsNullOrEmpty(role)
            ? throw new UnauthorizedAccessException("Роль не найдена в токене")
            : role;
    }
}