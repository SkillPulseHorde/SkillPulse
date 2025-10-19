using System.Text.Json.Serialization;
using AssessmentService.Api;
using AssessmentService.Api.Dto;
using AssessmentService.Application.Commands;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using AssessmentService.Infrastructure.Repos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#region di
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AssessmentDb")));

builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(CreateAssessmentCommandHandler).Assembly);
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); 
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
#endregion

#region endpoints
app.MapPost("/api/assessments", async (
    [FromBody] CreateAssessmentRequestDto request,
    IMediator mediator,
    CancellationToken ct) =>
{
    var command = new CreateAssessmentCommand
    {
        EvaluateeId = request.EvaluateeId,
        StartAt = request.StartAt,
        EndsAt = request.EndsAt,
        CreatedByUserId = request.CreatedByUserId,
    };
    
    var result = await mediator.Send(command, ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : result.Error.ToProblemDetails();
});

#endregion

app.Run();