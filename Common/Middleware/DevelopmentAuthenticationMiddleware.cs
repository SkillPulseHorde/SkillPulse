using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Common.Middleware;

public class DevelopmentAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    
    private const string DevUserId = "55555555-5555-5555-5555-555555555001";
    private const string DevUserRole = "HR";

    public DevelopmentAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        // Для перехода в авторизацию сервиса, написать "1" в авторизации
        if (context.Request.Headers.TryGetValue("Authorization", out var serviceKey) &&
            serviceKey == "Bearer 1")
        {
            var serviceClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000"),
                new Claim(ClaimTypes.Role, "InternalService"),
                new Claim("auth_type", "service"),
                new Claim("dev_mode", "true")
            };

            var serviceIdentity = new ClaimsIdentity(serviceClaims, "DevServiceAuth");
            context.User = new ClaimsPrincipal(serviceIdentity);

            await _next(context);
            return;
        }

        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, DevUserId),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, DevUserRole),
                new Claim("dev_mode", "true"),
            };

            var identity = new ClaimsIdentity(claims, "DevAuth");
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;
        }

        await _next(context);
    }
}