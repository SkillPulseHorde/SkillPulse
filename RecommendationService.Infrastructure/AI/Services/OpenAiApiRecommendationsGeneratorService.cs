using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RecommendationService.Application.AiServiceAbstract;
using RecommendationService.Application.Exceptions;
using RecommendationService.Application.Models;
using RecommendationService.Infrastructure.AI.Configuration;
using RecommendationService.Infrastructure.Dto;

namespace RecommendationService.Infrastructure.AI.Services;

public class OpenAiApiRecommendationsGeneratorService : IAiRecommendationsGeneratorService
{
    private readonly Kernel _kernel;
    private readonly RecommendationsAiOptions _settings;

    public OpenAiApiRecommendationsGeneratorService(IOptions<RecommendationsAiOptions> settings)
    {
        _settings = settings.Value;

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: _settings.Model,
            apiKey: _settings.ApiKey,
            endpoint: new Uri(_settings.BaseUrl));
        _kernel = builder.Build();
    }

    public async Task<RecommendationsModel> GenerateRecommendationsAsync(List<CompetenceWithResultModel> model,
        CancellationToken ct = default)
    {
        try
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
                throw new AiInvalidResponseException("AI сервис вернул пустой ответ");

            var dto = RecommendationsDeserialize(aiResponse);

            var recommendationModel = new RecommendationsModel()
            {
                RecommendationCompetences = dto.Select(a => a.ToRecommendationsCompetenceModel()).ToList()
            };

            return recommendationModel;
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException("Превышено время ожидания ответа от AI сервиса", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new AiTransientException("Ошибка при обращении к AI сервису. ", upstreamBody: ex.Message, inner: ex);
        }
        catch (HttpOperationException ex)
        {
            throw new HttpRequestException(ex.Message, inner: ex);
        }
    }

    private static List<RecommendationsResultFromAiDto> RecommendationsDeserialize(string aiResponse)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };

        try
        {
            var resultFromAiDtos = JsonSerializer.Deserialize<List<RecommendationsResultFromAiDto>>(aiResponse, options);
            if (resultFromAiDtos is not null && resultFromAiDtos.Count != 0)
                return resultFromAiDtos;

            throw new AiInvalidResponseException("Ошибка в формате ответа от AI сервиса", aiResponse);
        }
        catch (JsonException je)
        {
            throw new AiInvalidResponseException("Ошибка при десериализации ответа от AI сервиса",
                upstreamBody: aiResponse,
                inner: je);
        }
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
        sb.AppendLine("На вход будет подан валидный JSON. Правильно распарси его. ");
        sb.AppendLine("Возвращай ТОЛЬКО валидный JSON массив, без дополнительных символов и текста. ");
        sb.AppendLine("где IsPassedThreshold = true - в пункте wayToImproveCompetence надо похвалить ");
        sb.AppendLine("Если IsPassedThreshold = false - напиши способы улучшения (как описано в выходном формате) ");
        sb.AppendLine("Если IsPassedThreshold = null - верни пустым поле wayToImproveCompetence");
        sb.AppendLine(
            "Разделитель - §§ - используется только в случае, если нужно выделить несколько пунктов в wayToImproveCompetence. ");
        sb.AppendLine("Разделить может стоят ТОЛЬКО между пунктами, которые разделяет");
        sb.AppendLine("В других пунктах не используй разделитель. Где разделить используется - не используй маркеры списков (как и перечисление). ");
        sb.AppendLine("Входной формат для каждой компетенции: " +
                      "[\"competenceName\": \"название компетенции\", " +
                      "\"badCriteria\": [\"Список недостаточно развитых критериев через запятую\"]" +
                      "\"IsPassedThreshold\": \"булево значение. Пройден ли порог компетенции\"]");
        sb.AppendLine("Выходной формат для каждой компетенции: " +
                      "[\"competenceName\": \"название компетенции\", " +
                      "\"competenceReason\": \"объяснение, почему это важно для работы (работа в ИТ сфере)\", " +
                      "\"wayToImproveCompetence\": \"способы улучшения (самостоятельные) от 1 до 5, сколько сможешь. \"]");

        return sb.ToString();
    }
}