using System.Text;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RecommendationService.Application.AI;
using RecommendationService.Infrastructure.AI.Configuration;

namespace RecommendationService.Infrastructure.AI.Services;

public class OpenAiApiPlanGeneratorService : IAiPlanGeneratorService
{
    private readonly Kernel _kernel;
    private readonly AiOptions _settings;

    public OpenAiApiPlanGeneratorService(
        IOptions<AiOptions> settings,
        ILogger<OpenAiApiPlanGeneratorService> logger)
    {
        _settings = settings.Value;

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: _settings.Model,
            apiKey: _settings.ApiKey,
            endpoint: new Uri(_settings.BaseUrl));
        _kernel = builder.Build();
    }
    
    public async Task<Result<string>> GeneratePlanAsync(string assessmentData, CancellationToken ct = default)
    {

       try
       {
           var prompt = BuildPrompt(assessmentData);

           var chatService = _kernel.GetRequiredService<IChatCompletionService>();
           
           var chatHistory = new ChatHistory();
           chatHistory.AddSystemMessage("Ты эксперт в написании коротких и правильных ответах");
           chatHistory.AddUserMessage(prompt);

           var executionSettings = new OpenAIPromptExecutionSettings
           {
               Temperature = _settings.Temperature,
           };

           var response = await chatService.GetChatMessageContentAsync(
               chatHistory, 
               executionSettings, 
               _kernel, 
               ct);
           
           var aiResponse = response.Content ?? string.Empty;

           return Result<string>.Success(aiResponse);
       }
       catch (Exception ex)
       {
           return Result<string>.Failure(Error.NotFound("Ошибка генерации ИПР"));
       }
    }

    private string BuildPrompt(string assessmentData)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Тестовый запрос. Напиши ответ на следующий вопрос:");
        sb.AppendLine(assessmentData);
        // sb.AppendLine("");

        return sb.ToString();
    }
}