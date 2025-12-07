using AssessmentService.Api.Extensions;
using Common.Middleware;
using Common.Shared.Auth.Extensions;

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

var internalToken = app.Configuration.GetSection("Services:UserService:InternalToken")
                    ?? throw new InvalidOperationException("InternalToken не найден");
app.UseMiddleware<ServiceAuthenticationMiddleware>(internalToken.Value);

app.UseAuthentication();

if (app.Environment.IsDevelopment())
    app.UseMiddleware<DevelopmentAuthenticationMiddleware>();

app.UseAuthorization();

app.MapAllEndpoints();

app.Run();