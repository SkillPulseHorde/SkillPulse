using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Common.Middleware;

public sealed class ServiceAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _internalServiceKey;

    public ServiceAuthenticationMiddleware(
        RequestDelegate next,
        string? internalServiceKey)
    {
        _next = next;
        _internalServiceKey = internalServiceKey;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrEmpty(_internalServiceKey))
            return _next(context);

        var authHeader = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader))
            return _next(context);

        // Извлекаем токен
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..].Trim()
            : authHeader;

        if (token != _internalServiceKey)
            return _next(context);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000"),
            new Claim(ClaimTypes.Role, "InternalService"),
            new Claim("auth_type", "service")
        };

        var identity = new ClaimsIdentity(claims, "ServiceAuth");
        context.User = new ClaimsPrincipal(identity);

        return _next(context);
    }
}