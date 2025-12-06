using Common;
using MediatR;
using RecommendationService.Application.AiServiceAbstract;
using RecommendationService.Application.Models;
using RecommendationService.Application.ServiceClientsAbstract;
using RecommendationService.Domain.Entities;
using RecommendationService.Domain.Repos;
using RecommendationService.Domain.ValueObjects;

namespace RecommendationService.Application.Commands;

public sealed record GetRecommendationsByUserIdCommand : IRequest<Result<RecommendationModel?>>
{
    public required Guid UserId { get; init; }

    public required Guid AssessmentId { get; init; }
}

public sealed class GetRecommendationsByUserIdCommandHandler(
    IIndividualDevelopmentPlanRepository planRepository,
    IThresholdValueRepository thresholdValueRepository,
    ILearningMaterialRepository materialRepository,
    IAiLearningMaterialSearchService aiMaterialSearchService,
    IAiIprGeneratorService aiPlanGeneratorService,
    IUserServiceClient userServiceClient,
    IAssessmentServiceClient assessmentServiceClient)
    : IRequestHandler<GetRecommendationsByUserIdCommand, Result<RecommendationModel?>>
{
    public async Task<Result<RecommendationModel?>> Handle(
        GetRecommendationsByUserIdCommand request,
        CancellationToken ct)
    {
        var ipr = await planRepository.GetByAssessmentIdAsync(request.AssessmentId, ct);
        if (ipr is not null)
             return RecommendationModel.Deserialize(ipr.SummaryJson);

        var user = await userServiceClient.GetUserByIdAsync(request.UserId, ct);
        if (user == null)
            return Error.NotFound("Пользователь не был найден");

        var assessmentResult = await assessmentServiceClient.GetAssessmentResult(request.AssessmentId, ct);
        if (assessmentResult == null)
            return Error.NotFound("Аттестация не была найдена");

        var competenceDefinitions = await assessmentServiceClient.GetCompetenciesByEvaluateeId(request.UserId, ct);
        if (competenceDefinitions.Count == 0)
            return Error.NotFound("Не найдены подходящие компетенции");

        var threshold = await thresholdValueRepository.GetThresholdValueByGrade(user.Grade, ct);
        if (threshold == null)
            return Error.NotFound("Неопознанный grade у пользователя");

        var competenceResult = BuildCompetenceResults(assessmentResult, competenceDefinitions, threshold);

        var recommendation = new RecommendationModel();
        for (var i = 1; recommendation == null && i < 3; i++)
        {
            recommendation = await aiPlanGeneratorService.GenerateIprAsync(competenceResult, ct);
        }
        if (recommendation == null)
            return Error.Conflict("Не удалось сгенерировать план развития");

        var learningMaterialTags = Enum.GetNames<LearningMaterialTag>().ToList();

        foreach (var iprCompetenceModel in recommendation.RecommendationCompetences)
        {
            var competenceName = iprCompetenceModel.CompetenceName;
            var learningMaterials =
                await materialRepository.GetByCompetenceAsync(competenceName, learningMaterialTags, ct);

            if (learningMaterials != null)
            {
                iprCompetenceModel.LearningMaterials = learningMaterials
                    .Select(m => new LearningMaterialModel
                    {
                        LearningMaterialName = m.Title,
                        LearningMaterialType = m.Tag.ToString(),
                        LearningMaterialUrl = m.Url ?? string.Empty
                    })
                    .ToList();
            }

            var learningMaterialsAi =
                await aiMaterialSearchService.SearchLearningMaterialsAsync(competenceName, learningMaterialTags, ct);
            if (learningMaterialsAi == null)
                return new Result<RecommendationModel?>();

            iprCompetenceModel.LearningMaterials = learningMaterialsAi;
            var materialsToSave = learningMaterialsAi
                .Select(m => new LearningMaterial
                {
                    Title = m.LearningMaterialName,
                    Tag = Enum.Parse<LearningMaterialTag>(m.LearningMaterialType),
                    Url = m.LearningMaterialUrl,
                    CompetenceName = competenceName,
                    Id = Guid.NewGuid(),
                    Created = DateTimeOffset.UtcNow
                })
                .ToList();
            await materialRepository.AddRangeAsync(materialsToSave, ct);
        }

        await planRepository.CreateAsync(new IndividualDevelopmentPlan
        {
            AssessmentId = request.AssessmentId,
            SummaryJson = recommendation.Serialize()
        }, ct);

        return recommendation;
    }

    private static List<CompetenceWithResultModel> BuildCompetenceResults(
        AssessmentResultModel assessmentResult,
        List<CompetenceModel> competencesForEvaluatee,
        ThresholdValue thresholdValue)
    {
        var results = new List<CompetenceWithResultModel>();

        foreach (var (competenceId, competence) in assessmentResult.CompetenceSummaries)
        {
            var competenceInfo = competencesForEvaluatee.FirstOrDefault(c => c.Id == competenceId);
            if (competenceInfo == null)
                continue;

            var criterionResult = new List<CriterionWithResultModel>();
            if (competence is not null)
            {
                foreach (var (criterionId, criterion) in competence.CriterionSummaries)
                {
                    var criterionInfo = competenceInfo.Criteria.FirstOrDefault(c => c.Id == criterionId);

                    criterionResult.Add(new CriterionWithResultModel
                    {
                        CriterionName = criterionInfo?.Name ?? "Неизвестный критерий",
                        CriterionScore = criterion.Score,
                        IsPassedThreshold = criterion.Score >= (criterionInfo?.IsMandatory == true
                            ? thresholdValue.MinCoreThreshold
                            : thresholdValue.MinAvgCriterion)
                    });
                }
            }

            results.Add(new CompetenceWithResultModel
            {
                CompetenceName = competenceInfo.Name,
                CompetenceAvgScore = competence?.AverageScore,
                IsPassedThreshold = competence is not null
                    ? competence.AverageScore >= thresholdValue.MinAvgCompetence
                    : null,
                Criteria = criterionResult
            });
        }

        return results;
    }
}