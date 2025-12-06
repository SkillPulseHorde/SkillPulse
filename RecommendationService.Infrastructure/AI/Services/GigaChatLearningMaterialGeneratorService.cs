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

public sealed class GigaChatLearningMaterialGeneratorService : IAiLearningMaterialSearchService
{
    private const int Timeout = 30;
    private const string AuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

    private readonly LearningMaterialAiOptions _settings;

    private string? _accessToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public GigaChatLearningMaterialGeneratorService(
        IOptions<LearningMaterialAiOptions> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Генерация запроса
    /// </summary>
    public async Task<List<LearningMaterialModel>?> SearchLearningMaterialsAsync(
        string competence,
        List<string> tags,
        CancellationToken ct = default)
    {
        var accessToken = await GetAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(accessToken))
            return null;

        var kernel = CreateKernelWithToken(accessToken);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(GetSystemMessage(tags));
        chatHistory.AddUserMessage(BuildPrompt(competence, tags));

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = _settings.Temperature
        };

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel,
            ct);
        var aiResponseJson = response.Content;
        if (string.IsNullOrEmpty(aiResponseJson))
            return null;

        var dto = LearningMaterialDeserialize(aiResponseJson);

        var materials = dto?.Select(resultFromAiDto =>
            new LearningMaterialModel()
            {
                LearningMaterialName = resultFromAiDto.Title,
                LearningMaterialType = resultFromAiDto.Type ?? string.Empty,
                LearningMaterialUrl = resultFromAiDto.Link ?? string.Empty,
            }).ToList();

        return materials;
    }

    private static List<MaterialSearchResultFromAiDto>? LearningMaterialDeserialize(string aiResponse)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };

        var materials = JsonSerializer.Deserialize<List<MaterialSearchResultFromAiDto>>(aiResponse, options);
        if (materials is not null && materials.Count != 0)
            return materials;

        return null;
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
        sb.AppendLine("Формат: " +
                      "[{\"title\": \"название\", " +
                      "\"author\": \"автор\", " +
                      $"\"type\": \"{string.Join(", ", tags)}\", " +
                      "\"link\": \"url\", " +
                      "\"description\": \"краткое описание\"}]");

        return sb.ToString();
    }
}