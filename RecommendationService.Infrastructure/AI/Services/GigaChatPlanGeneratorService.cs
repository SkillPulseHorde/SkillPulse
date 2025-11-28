using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Common;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RecommendationService.Application.AI;
using RecommendationService.Infrastructure.AI.Configuration;

namespace RecommendationService.Infrastructure.AI.Services;

public class GigaChatPlanGeneratorService : IAiPlanGeneratorService
{
    private const int Timeout = 30;
    private const string AuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

    private readonly AiOptions _settings;
    private readonly ILogger<OpenAiApiPlanGeneratorService> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public GigaChatPlanGeneratorService(
        ILogger<OpenAiApiPlanGeneratorService> logger,
        IOptions<AiOptions> settings)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Генерация запроса
    /// </summary>
    public async Task<Result<string>> GeneratePlanAsync(string assessmentData, CancellationToken ct = default)
    {
        _logger.LogInformation("Начало процесса работы с гегачатом");
        var accessToken = await GetAccessTokenAsync(ct);
        _logger.LogInformation("Токен: " + accessToken);

        if (string.IsNullOrEmpty(accessToken))
            return Result<string>.Failure(Error.NotFound("Не получен Аксес токен"));

        var prompt = BuildPrompt(assessmentData);
        _logger.LogInformation("Промпт: " + prompt);

        var kernel = CreateKernelWithToken(accessToken);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("Ты эксперт в написании коротких и правильных ответах");
        chatHistory.AddUserMessage(prompt);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = _settings.Temperature
        };

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel,
            ct);
        var aiResponse = response.Content ?? string.Empty;

        return Result<string>.Success(aiResponse);
    }

    /// <summary>
    /// Получение Access token для GigaChat через OAuth2
    /// </summary>
    private async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            return _accessToken;

        _logger.LogInformation("ТокенНаличие");

        var httpClient = CreateHttpClientWithCertificate();
        if (httpClient is null)
        {
            _logger.LogInformation("HttpClient is null");
            return null;
        }

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
        {
            _logger.LogInformation("Путь не найден");
            _logger.LogInformation(certPath);
            return null;
        }

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

    /// <summary>
    /// Создает UserPrompt
    /// </summary>
    private string BuildPrompt(string assessmentData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Тестовый запрос. Напиши ответ на следующий вопрос:");
        sb.AppendLine(assessmentData);
        return sb.ToString();
    }
}

public record GigaChatTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")] public long ExpiresAt { get; set; }
}