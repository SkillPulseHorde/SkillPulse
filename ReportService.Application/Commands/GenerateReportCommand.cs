using Common;
using FluentValidation;
using MediatR;
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
    IRecommendationServiceClient recommendationServiceClient)
    : IRequestHandler<GenerateReportCommand, Result<ReportModel>>
{
    public async Task<Result<ReportModel>> Handle(GenerateReportCommand request, CancellationToken ct)
    {
        try
        {
            var assessmentResult = await assessmentServiceClient.GetAssessmentResultByIdAsync(request.AssessmentId, ct);
            var names = await assessmentServiceClient.GetCompetencesAndCriteriaNamesAsync(ct);
            
            var employeeInfo = await userServiceClient.GetUserByIdAsync(request.EmployeeId, ct);
            
            var recommendations = await recommendationServiceClient
                .GetRecommendationsByAssessmentIdAsync(request.AssessmentId, request.EmployeeId, ct);

            var reportData = await reportGenerator.GenerateReportAsync(
                assessmentResult, 
                names, 
                recommendations,
                request.ChartImage, 
                employeeInfo.FullName, 
                ct);

            return new ReportModel
            {
                Data = reportData,
                EmployeeName = employeeInfo.FullName
            };
        }
        catch (HttpRequestException ex)
        {
            return Error.Conflict($"Не удалось получить данные для аттестации {request.AssessmentId}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Error.Conflict($"Ошибка при генерации отчёта: {ex.Message}");
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
