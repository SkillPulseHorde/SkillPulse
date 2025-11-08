using System.Security.Claims;

namespace UserService.Middleware;

// Только для разработки!
public class DevelopmentAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    
    public DevelopmentAuthenticationMiddleware(
        RequestDelegate next, 
        IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_environment.IsDevelopment() && 
            !context.Request.Headers.ContainsKey("Authorization"))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "55555555-5555-5555-5555-555555555001"),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "HR"),
                new Claim("dev_user", "true"),
            };

            var identity = new ClaimsIdentity(claims, "DevAuth");
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;
        }

        await _next(context);
    }
}