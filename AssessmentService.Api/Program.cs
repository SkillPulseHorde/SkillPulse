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
using Common.Shared.Auth.Extensions;
using Microsoft.OpenApi.Models;

#region di
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AssessmentDb")));

builder.Services.AddJwtAuthentication(options =>
{
    options.SecretKey = builder.Configuration["JWT_SECRET_KEY"] ?? "";
});
builder.Services.AddRoleBasedAuthorization();

builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AssessmentService API",
        Version = "v1",
        Description = "API для аутентификации и авторизации"
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен в формате: Bearer {ваш_токен}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseAuthentication();
app.UseAuthorization();

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
    })
    .RequireAuthorization("HROnly");

#endregion

app.Run();