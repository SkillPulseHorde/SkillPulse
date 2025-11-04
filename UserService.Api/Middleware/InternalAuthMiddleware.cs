using System.Security.Claims;

namespace UserService.Middleware;

public class InternalAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _internalToken;

    public InternalAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _internalToken = configuration["InternalToken"] // Локально, через AppSettings
                         ?? configuration["INTERNAL_TOKEN"] // В проде, через переменные окружения (env)
                         ?? throw new InvalidOperationException("INTERNAL_TOKEN не найден");
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        
        if (path?.StartsWith("/api/users/") == true)
        {
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                if (token == _internalToken)
                {
                    var claims = new[]
                    {
                        new Claim("sub", "00000000-0000-0000-0000-000000000000"),
                        new Claim(ClaimTypes.Role, "internal"),
                    };

                    var identity = new ClaimsIdentity(claims, "InternalAuth");
                    var principal = new ClaimsPrincipal(identity);
                    
                    context.User = principal;

                    await _next(context);
                    return;
                }
            }
        }
        
        await _next(context);
    }
}