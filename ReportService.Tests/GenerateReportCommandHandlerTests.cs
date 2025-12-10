using Microsoft.Extensions.Logging;
using Moq;
using ReportService.Application;
using ReportService.Application.Commands;
using ReportService.Application.Models;
using ReportService.Application.ServiceClientsAbstract;

namespace ReportService.Tests;

public class GenerateReportCommandHandlerTests
{
    private readonly Mock<IReportGenerator> _reportGeneratorMock = new();
    private readonly Mock<IAssessmentServiceClient> _assessmentServiceClientMock = new();
    private readonly Mock<IUserServiceClient> _userServiceClientMock = new();
    private readonly Mock<IRecommendationServiceClient> _recommendationServiceClientMock = new();
    private readonly Mock<ILogger<GenerateReportCommandHandler>> _loggerMock = new();

    private GenerateReportCommandHandler CreateSut()
        => new(
            _reportGeneratorMock.Object,
            _assessmentServiceClientMock.Object,
            _userServiceClientMock.Object,
            _recommendationServiceClientMock.Object,
            _loggerMock.Object);

    [Fact]
    public async Task Handle_AllParallelCallsSucceed_ReturnsReportModel()
    {
        // arrange
        var assessmentId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var assessmentResult = new AssessmentResultModel
        {
            CompetenceSummaries = new Dictionary<Guid, CompetenceSummaryModel?>()
        };

        var names = new CompetencesAndCriteriaNamesModel
        {
            CompetenceNames = new Dictionary<Guid, string>(),
            CriterionNames = new Dictionary<Guid, string>()
        };

        var user = new UserModel
        {
            Id = employeeId,
            FullName = "Иван Иванов",
            Position = "Developer",
            Grade = EmployeeGrade.J1
        };

        var recommendations = new RecommendationsModel();
        var reportBytes = new byte[] { 1, 2, 3 };

        _assessmentServiceClientMock
            .Setup(c => c.GetAssessmentResultByIdAsync(assessmentId, ct))
            .ReturnsAsync(assessmentResult);

        _assessmentServiceClientMock
            .Setup(c => c.GetCompetencesAndCriteriaNamesAsync(ct))
            .ReturnsAsync(names);

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(employeeId, ct))
            .ReturnsAsync(user);

        _recommendationServiceClientMock
            .Setup(c => c.GetRecommendationsByAssessmentIdAsync(assessmentId, employeeId, ct))
            .ReturnsAsync(recommendations);

        _reportGeneratorMock
            .Setup(g => g.GenerateReportAsync(assessmentResult, names, recommendations, It.IsAny<byte[]>(), user.FullName, ct))
            .ReturnsAsync(reportBytes);

        var handler = CreateSut();

        var command = new GenerateReportCommand
        {
            AssessmentId = assessmentId,
            EmployeeId = employeeId,
            ChartImage = [10, 20]
        };

        // act
        var result = await handler.Handle(command, ct);

        // assert
        Assert.True(result.IsSuccess);
        var report = result.Value;
        Assert.NotNull(report);
        Assert.Equal(reportBytes, report.Data);
        Assert.StartsWith("Assessment_Report_", report.FileName);
        Assert.Contains("Иван Иванов", report.FileName);
    }

    [Fact]
    public async Task Handle_OneParallelCallFails_PropagatesExceptionAsError()
    {
        // arrange
        var assessmentId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var ct = CancellationToken.None;

        _assessmentServiceClientMock
            .Setup(c => c.GetAssessmentResultByIdAsync(assessmentId, ct))
            .ThrowsAsync(new FileNotFoundException("assessment not found"));

        _assessmentServiceClientMock
            .Setup(c => c.GetCompetencesAndCriteriaNamesAsync(ct))
            .ReturnsAsync(new CompetencesAndCriteriaNamesModel
            {
                CompetenceNames = new Dictionary<Guid, string>(),
                CriterionNames = new Dictionary<Guid, string>()
            });

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(employeeId, ct))
            .ReturnsAsync(new UserModel
            {
                Id = employeeId,
                FullName = "Иван Иванов",
                Position = "Developer",
                Grade = EmployeeGrade.J1
            });

        var handler = CreateSut();

        var command = new GenerateReportCommand
        {
            AssessmentId = assessmentId,
            EmployeeId = employeeId,
            ChartImage = [10, 20]
        };

        // act
        var result = await handler.Handle(command, ct);

        // assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("assessment not found", result.Error.Message);
    }

    [Fact]
    public async Task Handle_HttpRequestExceptionInParallelCall_ReturnsServiceUnavailableWithSanitizedMessage()
    {
        // arrange
        var assessmentId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var ct = CancellationToken.None;

        _assessmentServiceClientMock
            .Setup(c => c.GetAssessmentResultByIdAsync(assessmentId, ct))
            .ThrowsAsync(new HttpRequestException("internal http details"));

        _assessmentServiceClientMock
            .Setup(c => c.GetCompetencesAndCriteriaNamesAsync(ct))
            .ReturnsAsync(new CompetencesAndCriteriaNamesModel
            {
                CompetenceNames = new Dictionary<Guid, string>(),
                CriterionNames = new Dictionary<Guid, string>()
            });

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(employeeId, ct))
            .ReturnsAsync(new UserModel
            {
                Id = employeeId,
                FullName = "Иван Иванов",
                Position = "Developer",
                Grade = EmployeeGrade.J1
            });

        var handler = CreateSut();

        var command = new GenerateReportCommand
        {
            AssessmentId = assessmentId,
            EmployeeId = employeeId,
            ChartImage = [10, 20]
        };

        // act
        var result = await handler.Handle(command, ct);

        // assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Не удалось получить данные для генерации отчёта. Попробуйте позже.", result.Error.Message);
    }
}
