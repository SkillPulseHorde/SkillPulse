using AssessmentService.Application.Models;
using AssessmentService.Domain;
using AssessmentService.Domain.Repos;
using AssessmentService.Domain.ValueObjects;
using Common;
using MediatR;

namespace AssessmentService.Application.Queries;

public sealed record GetAssessmentResultQuery(Guid AssessmentId) : IRequest<Result<AssessmentResultModel>>;

public sealed class GetAssessmentResultQueryHandler(
    IEvaluationAnalyzer evaluationAnalyzer,
    IAssessmentRepository assessmentRepository)
    : IRequestHandler<GetAssessmentResultQuery, Result<AssessmentResultModel>>
{
    public async Task<Result<AssessmentResultModel>> Handle(GetAssessmentResultQuery request, CancellationToken ct)
    {
        var assessment = await assessmentRepository.GetByIdReadonlyAsync(request.AssessmentId, ct);
        
        if (assessment is null)
            return Error.NotFound($"Аттестация с ID {request.AssessmentId} не найдена");
        
        var assessmentResult = await evaluationAnalyzer.GetAssessmentResultByAssessmentIdAsync(request.AssessmentId, ct);

        if (assessmentResult is null)
        {
            return new AssessmentResultModel
            {
                CompetenceSummaries = new Dictionary<Guid, CompetenceSummary?>()
            };
        }

        var model = new AssessmentResultModel
        {
            CompetenceSummaries = assessmentResult.Data.CompetenceSummaries
        };

        return model;
    }
}
