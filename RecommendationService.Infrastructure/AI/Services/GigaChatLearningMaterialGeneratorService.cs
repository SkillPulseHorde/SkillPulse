using System.Net;
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
using RecommendationService.Application.Exceptions;
using RecommendationService.Application.Models;
using RecommendationService.Infrastructure.AI.Configuration;
using RecommendationService.Infrastructure.Dto;

namespace RecommendationService.Infrastructure.AI.Services;

public class GigaChatLearningMaterialGeneratorService : IALearningMaterialsSearchService
{
    private const int Timeout = 30;
    private const string AuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

    private readonly LearningMaterialsAiOptions _settings;

    private string? _accessToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public GigaChatLearningMaterialGeneratorService(
        IOptions<LearningMaterialsAiOptions> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Генерация запроса
    /// </summary>
    public async Task<List<LearningMaterialModel>> SearchLearningMaterialsAsync(string competence,
        List<string> tags,
        CancellationToken ct = default)
    {
        var accessToken = await GetAccessTokenAsync(ct);

        var kernel = CreateKernelWithToken(accessToken);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(GetSystemMessage(tags));
        chatHistory.AddUserMessage(BuildPrompt(competence, tags));

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = _settings.Temperature
        };

        try
        {
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                kernel,
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
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
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

        var materials = JsonSerializer.Deserialize<List<MaterialSearchResultFromAiDto>>(aiResponse, options);
        if (materials is not null && materials.Count != 0)
            return materials;

        return null;
    }

    /// <summary>
    /// Получение Access token для GigaChat через OAuth2
    /// </summary>
    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            return _accessToken;

        var httpClient = CreateHttpClientWithCertificate();
        if (httpClient is null)
            throw new AiTransientException(
                "Не удалось создать HttpClient с сертификатом для GigaChat (файл отсутствует)");

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _settings.ApiKey);
        request.Headers.Add("RqUID", Guid.NewGuid().ToString());

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
        ]);

        request.Content = content;

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("Превышено время ожидания ответа от сервиса авторизации GigaChat", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new AiTransientException("Ошибка при вызове сервиса авторизации GigaChat",
                upstreamBody: ex.Message,
                inner: ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                throw new AiAuthException("Ошибка авторизации при получении токена GigaChat",
                    upstreamBody: await response.Content.ReadAsStringAsync(ct));

            throw new AiTransientException("Ошибка при получении токена GigaChat",
                upstreamBody: await response.Content.ReadAsStringAsync(ct));
        }

        GigaChatTokenResponseDto? tokenResponse;
        try
        {
            tokenResponse = await response.Content.ReadFromJsonAsync<GigaChatTokenResponseDto>(ct);
        }
        catch (JsonException ex)
        {
            throw new AiInvalidResponseException("Ошибка при разборе ответа от сервиса авторизации GigaChat",
                upstreamBody: await response.Content.ReadAsStringAsync(ct),
                inner: ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AiTransientException("Ошибка при получении токена GigaChat",
                upstreamBody: await response.Content.ReadAsStringAsync(ct),
                inner: ex);
        }

        if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            throw new AiAuthException("Ошибка авторизации при получении токена GigaChat: пустой токен",
                upstreamBody: await response.Content.ReadAsStringAsync(ct));

        try
        {
            _accessToken = tokenResponse.AccessToken;
            _tokenExpiration = DateTimeOffset.FromUnixTimeMilliseconds(tokenResponse.ExpiresAt)
                .UtcDateTime
                .AddSeconds(-60);
        }
        catch
        {
            _accessToken = tokenResponse.AccessToken;
            _tokenExpiration = DateTime.UtcNow.AddMinutes(5);
        }

        return _accessToken;
    }

    /// <summary>
    /// Создает HttpClient с загруженным сертификатом
    /// </summary>
    private static HttpClient CreateHttpClientWithCertificate()
    {
        var certPath = Path.Combine(AppContext.BaseDirectory, "AI", "Certificates", "russian_trusted_root_ca.cer");

        if (!File.Exists(certPath))
            throw new AiTransientException("Файл сертификата для GigaChat не найден: " + certPath);

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