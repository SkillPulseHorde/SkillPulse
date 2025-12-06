using AuthService.Api.Extensions;
using Common.Shared.Auth.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddRoleBasedAuthorization();
builder.Services.AddApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapAllEndpoints();

app.Run();