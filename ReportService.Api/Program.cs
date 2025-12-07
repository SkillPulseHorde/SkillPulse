using Common.Middleware;
using Common.Shared.Auth.Extensions;
using ReportService.Api.Extensions;
using ReportService.Api.Extensions.ServiceRegistration;
using ReportService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReportService.Api;
using ReportService.Api.Dto;
using ReportService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddRoleBasedAuthorization();
builder.Services.AddApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddUserServiceClient(builder.Configuration);
builder.Services.AddAssessmentServiceClient(builder.Configuration);
builder.Services.AddRecommendationServiceClient(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<InternalAuthMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseMiddleware<DevelopmentAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/reports/generate", async Task<IResult> (
        [FromForm] GenerateReportRequestDto request,
        IMediator mediator,
        CancellationToken ct) =>
    {
        if (request.ChartImage.Length == 0)
            return Results.BadRequest(new { error = "Изображение графика обязательно" });

        // Читаем файл в массив байтов
        using var memoryStream = new MemoryStream();
        await request.ChartImage.CopyToAsync(memoryStream, ct);
        var imageBytes = memoryStream.ToArray();

        var command = new GenerateReportCommand
        {
            AssessmentId = request.AssessmentId,
            EmployeeId = request.EmployeeId,
            ChartImage = imageBytes
        };

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
            return result.Error!.ToProblemDetails();

        return Results.File(
            result.Value.Data,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        $"Assessment_Report_{result.Value.EmployeeName.Replace(" ", "")}.docx");
    })
    .Produces<FileResult>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .WithSummary("Генерация отчёта по результатам аттестации")
    .WithDescription("Принимает ID аттестации и изображение графика (form-data), возвращает DOCX файл")
    //.RequireAuthorization("Authenticated")
    .DisableAntiforgery();

app.Run();