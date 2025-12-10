using Common.Shared.Auth.Extensions;
using RecommendationService.Api.Extensions;
using Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddRoleBasedAuthorization();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var internalToken = app.Configuration.GetSection("Services:UserService:InternalToken").Value
                    ?? throw new InvalidOperationException("InternalToken не найден");
app.UseMiddleware<ServiceAuthenticationMiddleware>(internalToken);

app.UseAuthentication();

if (app.Environment.IsDevelopment())
    app.UseMiddleware<DevelopmentAuthenticationMiddleware>();

app.UseAuthorization();

app.MapAllEndpoints();

app.Run();