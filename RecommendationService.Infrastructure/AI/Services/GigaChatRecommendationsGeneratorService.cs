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

public class GigaChatRecommendationsGeneratorService : IAiRecommendationsGeneratorService
{
    private const int Timeout = 30;
    private const string AuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

    private readonly RecommendationsAiOptions _settings;

    private string? _accessToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public GigaChatRecommendationsGeneratorService(
        IOptions<RecommendationsAiOptions> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Генерация запроса
    /// </summary>
    public async Task<RecommendationsModel> GenerateRecommendationsAsync(List<CompetenceWithResultModel> model,
        CancellationToken ct = default)
    {
        var accessToken = await GetAccessTokenAsync(ct);

        var kernel = CreateKernelWithToken(accessToken);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(GetSystemMessage());
        chatHistory.AddUserMessage(BuildPrompt(model));

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
                kernel,
                ct);

            var aiResponse = response.Content;
            if (string.IsNullOrEmpty(aiResponse))
                throw new AiInvalidResponseException("AI сервис вернул пустой ответ", upstreamBody: aiResponse);

            var dto = RecommendationsDeserialize(aiResponse);
            if (dto == null)
                throw new AiInvalidResponseException("AI сервис вернул некорректный ответ", upstreamBody: aiResponse);

            var recommendationModel = new RecommendationsModel()
            {
                RecommendationCompetences = dto.Select(a => a.ToRecommendationsCompetenceModel()).ToList()
            };

            return recommendationModel;
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
            throw new AiTransientException("Ошибка при обращении к AI сервису. ", upstreamBody: ex.Message, inner: ex);
        }
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
                "Не удалось создать HttpClient с сертификатом для GigaChat (Отсутствует файл сертификата)");

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _settings.ApiKey);
        request.Headers.Add("RqUID", Guid.NewGuid().ToString());

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
        ]);

        request.Content = content;

        using var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new AiAuthException("Ошибка аутентификации при получении access token от GigaChat. " +
                                          $"Код ответа: {response.StatusCode}, Тело ответа: {await response.Content.ReadAsStringAsync(ct)}");
            }

            throw new AiTransientException("Ошибка при получении access token от GigaChat. " +
                                           $"Код ответа: {response.StatusCode}, Тело ответа: {await response.Content.ReadAsStringAsync(ct)}");
        }

        GigaChatTokenResponseDto? tokenResponse;
        try
        {
            tokenResponse = await response.Content.ReadFromJsonAsync<GigaChatTokenResponseDto>(ct);
        }
        catch (JsonException ex)
        {
            throw new AiInvalidResponseException("Ошибка при десериализации ответа с access token от GigaChat",
                upstreamBody: await response.Content.ReadAsStringAsync(ct),
                inner: ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AiTransientException("Неожиданная ошибка при получении access token от GigaChat",
                upstreamBody: await response.Content.ReadAsStringAsync(ct),
                inner: ex);
        }

        if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            throw new AiAuthException("Получен некорректный ответ с access token от GigaChat. " +
                                      $"Тело ответа: {await response.Content.ReadAsStringAsync(ct)}");

        try
        {
            _accessToken = tokenResponse.AccessToken;
            _tokenExpiration = DateTimeOffset.FromUnixTimeMilliseconds(tokenResponse.ExpiresAt)
                .UtcDateTime
                .AddSeconds(-60);
        }
        catch (Exception)
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
            throw new AiTransientException("Не удалось найти файл сертификата для GigaChat по пути: " + certPath);

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

    private static List<RecommendationsResultFromAiDto>? RecommendationsDeserialize(string aiResponse)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };

        try
        {
            var resultFromAiDtos =
                JsonSerializer.Deserialize<List<RecommendationsResultFromAiDto>>(aiResponse, options);
            if (resultFromAiDtos is not null && resultFromAiDtos.Count != 0)
                return resultFromAiDtos;

            return null;
        }
        catch (JsonException ex)
        {
            throw new AiInvalidResponseException("Ошибка при десериализации ответа от AI сервиса",
                upstreamBody: aiResponse,
                inner: ex);
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
        sb.AppendLine("Входной формат для каждой компетенции: " +
                      "[\"competenceName\": \"название компетенции\", " +
                      "\"badCriteria\": [\"Список недостаточно развитых критериев через запятую\"]" +
                      "\"IsPassedThreshold\": \"булево значение. Пройден ли порог компетенции\"]");
        sb.AppendLine("Выходной формат для каждой компетенции: " +
                      "[\"competenceName\": \"название компетенции\", " +
                      "\"competenceReason\": \"объяснение, почему это важно для работы (работа в ИТ сфере)\", " +
                      "\"wayToImproveCompetence\": \"способы улучшения (самостоятельные) от 1 до 5, сколько сможешь. Разделитель - §§\"]");

        return sb.ToString();
    }
}