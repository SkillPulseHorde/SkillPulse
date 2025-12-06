using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RecommendationService.Application.AiServiceAbstract;
using RecommendationService.Application.Models;
using RecommendationService.Infrastructure.AI.Configuration;
using RecommendationService.Infrastructure.Dto;

namespace RecommendationService.Infrastructure.AI.Services;

public class GigaChatPlanGeneratorService : IAiIprGeneratorService
{
    private const int Timeout = 30;
    private const string AuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

    private readonly IprAiOptions _settings;

    private string? _accessToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public GigaChatPlanGeneratorService(
        IOptions<IprAiOptions> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Генерация запроса
    /// </summary>
    public async Task<RecommendationModel?> GenerateIprAsync(
        List<CompetenceWithResultModel> model,
        CancellationToken ct = default)
    {
        var accessToken = await GetAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(accessToken))
            return null;

        var kernel = CreateKernelWithToken(accessToken);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(GetSystemMessage());
        chatHistory.AddUserMessage(BuildPrompt(model));

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = _settings.Temperature
        };

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel,
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

    /// <summary>
    /// Получение Access token для GigaChat через OAuth2
    /// </summary>
    private async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            return _accessToken;

        var httpClient = CreateHttpClientWithCertificate();
        if (httpClient is null)
            return null;

        var request = new HttpRequestMessage(HttpMethod.Post, AuthUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _settings.ApiKey);
        request.Headers.Add("RqUID", Guid.NewGuid().ToString());

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
        ]);

        request.Content = content;

        var response = await httpClient.SendAsync(request, ct);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GigaChatTokenResponse>(ct);
        if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            return null;

        _accessToken = tokenResponse.AccessToken;
        _tokenExpiration = DateTimeOffset.FromUnixTimeMilliseconds(tokenResponse.ExpiresAt)
            .UtcDateTime
            .AddSeconds(-60);

        return _accessToken;
    }

    /// <summary>
    /// Создает HttpClient с загруженным сертификатом
    /// </summary>
    private HttpClient? CreateHttpClientWithCertificate()
    {
        var certPath = Path.Combine(AppContext.BaseDirectory, "AI", "Certificates", "russian_trusted_root_ca.cer");

        if (!File.Exists(certPath))
            return null;

        var certificate = X509CertificateLoader.LoadCertificateFromFile(certPath);

        var handle = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, cert, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                    return true;

                if (cert == null || chain == null)
                    return false;

                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(certificate);

                return chain.Build(cert);
            }
        };

        return new HttpClient(handle)
        {
            Timeout = TimeSpan.FromSeconds(Timeout)
        };
    }

    /// <summary>
    /// Создает Kernel с настроенным HttpClient и accessToken
    /// </summary>
    /// <param name="accessToken"></param>
    private Kernel CreateKernelWithToken(string accessToken)
    {
        var httpClient = CreateHttpClientWithCertificate();

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: _settings.Model,
            apiKey: accessToken,
            endpoint: new Uri(_settings.BaseUrl),
            httpClient: httpClient);

        return builder.Build();
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