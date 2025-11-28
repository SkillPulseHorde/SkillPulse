using Common.Shared.Auth.Extensions;
using Microsoft.AspNetCore.Mvc;
using RecommendationService.Api.Extensions.DependencyInjection;
using RecommendationService.Application.Commands;
using MediatR;
using RecommendationService.Api;
using RecommendationService.Api.Dto;

#region di

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

// if (app.Environment.IsDevelopment())
//     app.UseMiddleware<DevelopmentAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region endpoints

app.MapPost("/api/recommendation", async (
        [FromBody] GetRecommendationRequestDto request,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var command = new GetRecommendationsByUserIdCommand(request.AssessmentId);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemDetails();
    })
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .WithName("Получить рекомендацию для конкретного пользователя")
    .AllowAnonymous();

#endregion

app.Run();