using Common;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ReportService.Application.Models;
using ReportService.Application.ServiceClientsAbstract;

namespace ReportService.Application.Commands;

public sealed record GenerateReportCommand : IRequest<Result<ReportModel>>
{
    public required Guid AssessmentId { get; init; }
    public required Guid EmployeeId { get; init; }
    public required byte[] ChartImage { get; init; }
}

public sealed class GenerateReportCommandHandler(
    IReportGenerator reportGenerator,
    IAssessmentServiceClient assessmentServiceClient,
    IUserServiceClient userServiceClient,
    IRecommendationServiceClient recommendationServiceClient,
    ILogger<GenerateReportCommandHandler> logger)
    : IRequestHandler<GenerateReportCommand, Result<ReportModel>>
{
    public async Task<Result<ReportModel>> Handle(GenerateReportCommand request, CancellationToken ct)
    {
        try
        {
            // Запускаем независимые вызовы параллельно
            var assessmentTask = assessmentServiceClient.GetAssessmentResultByIdAsync(request.AssessmentId, ct);
            var namesTask = assessmentServiceClient.GetCompetencesAndCriteriaNamesAsync(ct);
            var employeeTask = userServiceClient.GetUserByIdAsync(request.EmployeeId, ct);

            await Task.WhenAll(assessmentTask, namesTask, employeeTask);

            var assessmentResult = await assessmentTask;
            var names = await namesTask;
            var employeeInfo = await employeeTask;

            var recommendations = await recommendationServiceClient
                .GetRecommendationsByAssessmentIdAsync(request.AssessmentId, request.EmployeeId, ct);

            var reportData = await reportGenerator.GenerateReportAsync(
                assessmentResult,
                names,
                recommendations,
                request.ChartImage,
                employeeInfo.FullName,
                ct);
            
            // Очищаем имя сотрудника от недопустимых для имени файла символов
            var invalidChars = Path.GetInvalidFileNameChars();
            var employeeName = new string(employeeInfo.FullName
                .Where(c => !invalidChars.Contains(c))
                .ToArray());

            if (string.IsNullOrWhiteSpace(employeeName))
                employeeName = "Employee";

            var safeFileName = $"Assessment_Report_{employeeName}.docx";

            return new ReportModel
            {
                Data = reportData,
                FileName = safeFileName
            };
        }
        catch (FileNotFoundException ex)
        {
            return Error.NotFound(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Ошибка получения данных для формирования отчета. AssessmentId: {AssessmentId}, EmployeeId: {EmployeeId}",
                request.AssessmentId, request.EmployeeId);
            return Error.ServiceUnavailable("Не удалось получить данные для генерации отчёта. Попробуйте позже.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Неожиданная ошибка в процессе формирования отчета. AssessmentId: {AssessmentId}, EmployeeId: {EmployeeId}",
                request.AssessmentId, request.EmployeeId);
            return Error.InternalServerError("Ошибка при генерации отчёта. Попробуйте позже.");
        }
    }
}

public sealed class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(x => x.AssessmentId)
            .NotEmpty()
            .WithMessage("ID аттестации обязателен");

        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage("ID сотрудника обязателен");

        RuleFor(x => x.ChartImage)
            .NotEmpty()
            .WithMessage("Изображение графика обязательно")
            .Must(IsValidImageSize)
            .WithMessage("Размер изображения не должен превышать 5 МБ");
    }

    private bool IsValidImageSize(byte[] image)
    {
        const int maxSizeInBytes = 5 * 1024 * 1024; // 5 MB
        return image.Length <= maxSizeInBytes;
    }
}
