using System. Security.Claims;
using Microsoft. AspNetCore.Http;

namespace Common.Middleware;

public class ServiceAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string?  _internalServiceKey;

    public ServiceAuthenticationMiddleware(
        RequestDelegate next,
        string? internalServiceKey)
    {
        _next = next;
        _internalServiceKey = internalServiceKey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (! string.IsNullOrEmpty(_internalServiceKey))
        {
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authHeader))
            {
                string token;

                // Извлекаем токен
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader. Substring("Bearer ".Length).Trim();
                }
                else
                {
                    token = authHeader;
                }

                if (token == _internalServiceKey)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000"),
                        new Claim(ClaimTypes.Role, "InternalService"),
                        new Claim("auth_type", "service")
                    };

                    var identity = new ClaimsIdentity(claims, "ServiceAuth");
                    context. User = new ClaimsPrincipal(identity);
                }
            }
        }

        await _next(context);
    }
}