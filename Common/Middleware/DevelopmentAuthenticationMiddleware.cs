using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Common.Middleware;

public class DevelopmentAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    
    private const string DevUserId = "55555555-5555-5555-5555-555555555001";
    private const string DevUserRole = "HR";
    
    public DevelopmentAuthenticationMiddleware(
        RequestDelegate next, 
        IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, DevUserId),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, DevUserRole),
                new Claim("dev_user", "true"),
            };

            var identity = new ClaimsIdentity(claims, "DevAuth");
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;
        }

        await _next(context);
    }
}