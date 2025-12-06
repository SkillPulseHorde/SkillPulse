using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RecommendationService.Application.AiServiceAbstract;
using RecommendationService.Application.Models;
using RecommendationService.Infrastructure.AI.Configuration;
using RecommendationService.Infrastructure.Dto;

namespace RecommendationService.Infrastructure.AI.Services;

public class GetPrompt : IAiIprGeneratorService
{
    private readonly Kernel _kernel;
    private readonly IprAiOptions _settings;

    public GetPrompt(IOptions<IprAiOptions> settings)
    {
        _settings = settings.Value;

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: _settings.Model,
            apiKey: _settings.ApiKey,
            endpoint: new Uri(_settings.BaseUrl));
        _kernel = builder.Build();
    }

    public async Task<RecommendationModel?> GenerateIprAsync(
        List<CompetenceWithResultModel> model,
        CancellationToken ct = default)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(GetSystemMessage());
        chatHistory.AddUserMessage(BuildPrompt(model));

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = _settings.Temperature,
            ResponseFormat = "json_object"
        };

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel,
            ct);

        var aiResponse = response.Content;
        if (string.IsNullOrEmpty(aiResponse))
            return null;

        var dto = IprDeserialize(aiResponse);
        if (dto == null)
            return null;

        var recommendationModel = new RecommendationModel()
        {
            RecommendationCompetences = dto.Select(a => a.ToIprCompetenceModel()).ToList()
        };

        return recommendationModel;
    }

    private static List<IprResultFromAiDto>? IprDeserialize(string aiResponse)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };

        var resultFromAiDtos = JsonSerializer.Deserialize<List<IprResultFromAiDto>>(aiResponse, options);
        if (resultFromAiDtos is not null && resultFromAiDtos.Count != 0)
            return resultFromAiDtos;

        return null;
    }

    private static string BuildPrompt(List<CompetenceWithResultModel> model)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Составь идеальный индивидуальный план развития. ");
        sb.AppendLine("Для рекомендации можешь использовать badCriteria, как то, что желательно тоже улучшить. ");
        sb.AppendLine("[");
        foreach (var competence in model)
        {
            sb.AppendLine("\"competenceName\": " + competence.CompetenceName);
            sb.AppendLine("\"badCriteria\": [" +
                          string.Join(", ", competence.Criteria.Where(x => !x.IsPassedThreshold)
                              .ToList()) + "]");
            sb.AppendLine("\"IsPassedThreshold\": " + competence.IsPassedThreshold);
        }

        sb.AppendLine("]");
        sb.AppendLine("Ответ дать на русском языке. ");

        return sb.ToString();
    }

    private static string GetSystemMessage()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Ты эксперт в составлении индивидуального плана развития. ");
        sb.AppendLine("На вход будет подан валидный JSON. Правильно распарсь его. ");
        sb.AppendLine("Возвращай ТОЛЬКО валидный JSON массив, без дополнительных символов и текста. ");
        sb.AppendLine("где IsPassedThreshold = true - в пункте wayToImproveCompetence надо похвалить ");
        sb.AppendLine("Если IsPassedThreshold = false - напиши способы улучшения (как описано в выходном формате) ");
        sb.AppendLine("Если IsPassedThreshold = null - верни пустым поле wayToImproveCompetence");
        sb.AppendLine("Входной формат для каждой компетенции: " +
                      "[\"competenceName\": \"название компетенции\", " +
                      "\"badCriteria\": [\"Список недостаточно развитых критериев через запятую\"]" +
                      "\"IsPassedThreshold\": \"булево значение. Пройден ли порог компетенции\"]");
        sb.AppendLine("Выходной формат для каждой компетенции: " +
                      "[\"competenceName\": \"название компетенции\", " +
                      "\"competenceReason\": \"объяснение, почему это важно для работы (работа в ИТ сфере)\", " +
                      "\"wayToImproveCompetence\": \"способы улучшения (самостоятельные) от 1 до 5, сколько сможешь. Разделитель - §§\"]" +
                      "\"isEvaluated\": \"false - если IsPassedThreshold = null, иначе true\"]");

        return sb.ToString();
    }
}