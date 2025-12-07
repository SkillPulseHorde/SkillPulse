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

public class OpenAiApiLearningMaterialGeneratorService : IALearningMaterialsSearchService
{
    private readonly Kernel _kernel;
    private readonly LearningMaterialsAiOptions _settings;

    public OpenAiApiLearningMaterialGeneratorService(
        IOptions<LearningMaterialsAiOptions> settings)
    {
        _settings = settings.Value;

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: _settings.Model,
            apiKey: _settings.ApiKey,
            endpoint: new Uri(_settings.BaseUrl));
        _kernel = builder.Build();
    }

    public async Task<List<LearningMaterialModel>> SearchLearningMaterialsAsync(string competence,
        List<string> tags,
        CancellationToken ct = default)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(GetSystemMessage(tags));
        chatHistory.AddUserMessage(BuildPrompt(competence, tags));

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = _settings.Temperature,
            ResponseFormat = "json_object"
        };

        try
        {
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                ct);

            var aiResponseJson = response.Content;
            if (string.IsNullOrWhiteSpace(aiResponseJson))
                throw new AiInvalidResponseException("AI вернула пустой ответ", upstreamBody: aiResponseJson);

            var dto = LearningMaterialDeserialize(aiResponseJson);
            if (dto is null || dto.Count == 0)
                return [];

            var materials = dto.Select(resultFromAiDto =>
                new LearningMaterialModel()
                {
                    LearningMaterialName = resultFromAiDto.Title,
                    LearningMaterialType = resultFromAiDto.Type ?? string.Empty,
                    LearningMaterialUrl = resultFromAiDto.Link ?? string.Empty,
                }).ToList();

            return materials;
        }
        catch (TaskCanceledException ex) when (ct.IsCancellationRequested)
        {
            throw new TimeoutException("Превышено время ожидания ответа от AI сервиса", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new AiTransientException("Ошибка при вызове AI сервиса",
                upstreamBody: ex.Message,
                inner: ex);
        }
        catch (JsonException ex)
        {
            throw new AiInvalidResponseException("Ошибка при разборе ответа от AI сервиса",
                upstreamBody: ex.Message,
                inner: ex);
        }
    }

    private static List<MaterialSearchResultFromAiDto>? LearningMaterialDeserialize(string aiResponse)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };

        try
        {
            var materials = JsonSerializer.Deserialize<List<MaterialSearchResultFromAiDto>>(aiResponse, options);
            if (materials is not null && materials.Count != 0)
                return materials;

            return null;
        }
        catch (JsonException ex)
        {
            throw new AiInvalidResponseException("Ошибка при разборе ответа от AI сервиса",
                upstreamBody: ex.Message,
                inner: ex);
        }
    }

    private static string BuildPrompt(string competence, List<string> tags)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Подбери 3 наиболее релевантных и актуальных образовательных материалов.");
        sb.AppendLine("По одному виду материала: " + string.Join(", ", tags) + ". ");
        sb.AppendLine($"Материалы для развития компетенции: \"{competence}\". ");
        sb.AppendLine("Убедись, что ссылки рабочие и материалы доступны в этом году. ");
        sb.AppendLine("Желательно результаты на русском языке. Но, если нет на русском, могут быть и на английском.");

        return sb.ToString();
    }

    private static string GetSystemMessage(List<string> tags)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Ты эксперт в подборе образовательных материалов. ");
        sb.AppendLine("Возвращай ТОЛЬКО валидный JSON массив без дополнительного текста. ");
        sb.AppendLine("Ссылки должны быть рабочие и актуальные. Не надо придумывать ссылки самому. ");
        sb.AppendLine("Формат: " +
                      "[{\"title\": \"название\", " +
                      "\"author\": \"автор\", " +
                      $"\"type\": \"{string.Join(", ", tags)}\", " +
                      "\"link\": \"url\", " +
                      "\"description\": \"краткое описание\"}]");

        return sb.ToString();
    }
}