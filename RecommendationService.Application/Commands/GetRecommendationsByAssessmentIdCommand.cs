using Common;
using MediatR;
using RecommendationService.Application.AiServiceAbstract;
using RecommendationService.Application.Models;
using RecommendationService.Application.ServiceClientsAbstract;
using RecommendationService.Domain.Entities;
using RecommendationService.Domain.Repos;

namespace RecommendationService.Application.Commands;

public sealed record GetRecommendationsByAssessmentIdCommand : IRequest<Result<RecommendationsModel?>>
{
    public required Guid UserId { get; init; }

    public required Guid AssessmentId { get; init; }
}

public sealed class GetRecommendationsByAssessmentIdCommandHandler(
    IIndividualDevelopmentPlanRepository planRepository,
    IThresholdValueRepository thresholdValueRepository,
    ILearningMaterialRepository materialRepository,
    IALearningMaterialsSearchService aiMaterialSearchService,
    IAiRecommendationsGeneratorService aiPlanGeneratorService,
    IUserServiceClient userServiceClient,
    IAssessmentServiceClient assessmentServiceClient)
    : IRequestHandler<GetRecommendationsByAssessmentIdCommand, Result<RecommendationsModel?>>
{
    public async Task<Result<RecommendationsModel?>> Handle(
        GetRecommendationsByAssessmentIdCommand request,
        CancellationToken ct)
    {
        var recommendations = await planRepository.GetByAssessmentIdAsync(request.AssessmentId, ct);
        if (recommendations is not null)
            return RecommendationsModel.Deserialize(recommendations.SummaryJson);

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

        RecommendationsModel recommendation;
        try
        {
            recommendation = await aiPlanGeneratorService.GenerateRecommendationsAsync(competenceResult, ct);
        }
        catch (Exception ex)
        {
            var error = ExceptionToErrorMapper.Map(ex);
            return error;
        }

        var learningMaterialTags = Enum.GetNames<LearningMaterialTag>().ToList();

        var isCompetencePassedByName = competenceResult.ToDictionary(x => x.CompetenceName, x => x.IsPassedThreshold);

        var filteredCompetences = recommendation.RecommendationCompetences
            .Where(rc => rc.IsEvaluated
                         && !(isCompetencePassedByName.TryGetValue(rc.CompetenceName, out var isPassedThreshold) &&
                              isPassedThreshold == true));

        foreach (var recommendationsCompetenceModel in filteredCompetences)
        {
            var competenceName = recommendationsCompetenceModel.CompetenceName;

            var dbMaterials =
                await materialRepository.GetByCompetenceAsync(competenceName, learningMaterialTags, ct);
            if (dbMaterials != null && dbMaterials.Count != 0)
            {
                recommendationsCompetenceModel.LearningMaterials = dbMaterials
                    .Select(m => new LearningMaterialModel
                    {
                        LearningMaterialName = m.Title,
                        LearningMaterialType = m.Tag.ToString(),
                        LearningMaterialUrl = m.Url ?? string.Empty
                    })
                    .ToList();
                continue;
            }

            List<LearningMaterialModel> learningMaterialsAi;
            try
            {
                learningMaterialsAi =
                    await aiMaterialSearchService.SearchLearningMaterialsAsync(competenceName, learningMaterialTags,
                        ct);
            }
            catch (Exception ex)
            {
                var error = ExceptionToErrorMapper.Map(ex);
                return error;
            }

            if (learningMaterialsAi.Count == 0)
                continue;

            recommendationsCompetenceModel.LearningMaterials = learningMaterialsAi;
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

            if (materialsToSave.Count != 0)
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

            var areAllCriteriaPassed = criterionResult.All(c => c.IsPassedThreshold);

            results.Add(new CompetenceWithResultModel
            {
                CompetenceName = competenceInfo.Name,
                CompetenceAvgScore = competence?.AverageScore,
                IsPassedThreshold = !areAllCriteriaPassed
                    ? false
                    : competence is not null
                        ? competence.AverageScore >= thresholdValue.MinAvgCompetence
                        : null,
                Criteria = criterionResult
            });
        }

        return results;
    }
}