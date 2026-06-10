using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartSearch.Models.Search;

namespace SmartSearch.Services.Ai;

public class GigaChatService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<GigaChatService> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public GigaChatService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<GigaChatService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<(AiAnalysis analysis, string normalizedQuery)> AnalyzeQueryAsync(string userQuery)
    {
        var systemPrompt = @"Ты ассистент для анализа поисковых запросов товаров интернет-магазина на русском языке.
Товары в базе данных находятся на АНГЛИЙСКОМ языке.

Твоя задача:
1. Понять смысл запроса, исправить опечатки, расшифровать сленг и диалектизмы
2. Извлечь параметры поиска
3. ПЕРЕВЕСТИ нормализованный запрос на АНГЛИЙСКИЙ язык для поиска в базе

Отвечай ТОЛЬКО валидным JSON объектом:

{
  ""productType"": ""тип товара на АНГЛИЙСКОМ (t-shirt, jeans, jacket, dress, sneakers, sweater, coat, skirt, shirt, pants, shorts, hoodie, sweatshirt)"",
  ""color"": ""цвет на АНГЛИЙСКОМ (red, blue, black, white, green, yellow, pink, gray, brown, beige)"",
  ""size"": ""размер (XS, S, M, L, XL, XXL, 42-56)"",
  ""gender"": ""male/female/unisex/kids"",
  ""material"": ""материал на АНГЛИЙСКОМ (cotton, leather, polyester, wool, denim, cashmere)"",
  ""season"": ""сезон на АНГЛИЙСКОМ (winter, summer, spring, autumn, all-season)"",
  ""features"": [""особенности на АНГЛИЙСКОМ (warm, waterproof, hooded, printed, striped, checked, oversized)""],
  ""normalizedQueryRu"": ""нормализованный запрос на РУССКОМ"",
  ""normalizedQueryEn"": ""переведенный запрос на АНГЛИЙСКОМ для поиска""
}
";

        try
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("GigaChat token unavailable, using fallback NLP");
                return FallbackAnalysis(userQuery);
            }

            var apiUrl = _config["GigaChat:ApiUrl"] ?? "https://gigachat.devices.sberbank.ru/api/v1";
            var client = _httpFactory.CreateClient("GigaChat");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new
            {
                model = "GigaChat",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userQuery }
                },
                temperature = 0.1,
                max_tokens = 500
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{apiUrl}/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GigaChat API returned {StatusCode}", response.StatusCode);
                return FallbackAnalysis(userQuery);
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<GigaChatResponse>(responseText);
            var messageContent = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

            _logger.LogInformation("GigaChat response: {Response}", messageContent);

            var analysis = ParseAnalysisJson(messageContent);

            var normalizedQuery = !string.IsNullOrEmpty(analysis.NormalizedQueryEn)
                ? analysis.NormalizedQueryEn
                : (!string.IsNullOrEmpty(analysis.NormalizedQueryRu)
                    ? analysis.NormalizedQueryRu
                    : userQuery);

            return (analysis, normalizedQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GigaChat request failed");
            return FallbackAnalysis(userQuery);
        }
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            return _accessToken;

        await _tokenLock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                return _accessToken;

            var clientId = _config["GigaChat:ClientId"];
            var clientSecret = _config["GigaChat:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("GigaChat credentials not configured");
                return null;
            }

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            var authUrl = _config["GigaChat:AuthUrl"] ?? "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
            var scope = _config["GigaChat:Scope"] ?? "GIGACHAT_API_USSURSKY";

            var authClient = _httpFactory.CreateClient("GigaChatAuth");
            authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            authClient.DefaultRequestHeaders.Add("RqUID", Guid.NewGuid().ToString());

            var formData = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("scope", scope) });
            var authResponse = await authClient.PostAsync(authUrl, formData);

            if (!authResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("GigaChat auth failed: {StatusCode}", authResponse.StatusCode);
                return null;
            }

            var authText = await authResponse.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GigaChatTokenResponse>(authText);

            _accessToken = tokenResponse?.AccessToken;
            _tokenExpiry = tokenResponse?.ExpiresAt > 0
                ? DateTimeOffset.FromUnixTimeMilliseconds(tokenResponse.ExpiresAt).UtcDateTime.AddMinutes(-1)
                : DateTime.UtcNow.AddMinutes(29);

            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    // ✅ Убираем static, чтобы использовать _logger
    private AiAnalysis ParseAnalysisJson(string raw)
    {
        try
        {
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start >= 0 && end > start)
                raw = raw[start..(end + 1)];

            _logger.LogInformation("Parsing JSON: {Raw}", raw);

            var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var features = new List<string>();
            if (root.TryGetProperty("features", out var featEl) && featEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in featEl.EnumerateArray())
                    if (f.ValueKind == JsonValueKind.String)
                        features.Add(f.GetString() ?? "");
            }

            return new AiAnalysis
            {
                ProductType = root.TryGetProperty("productType", out var pt) ? pt.GetString() ?? "" : "",
                Color = root.TryGetProperty("color", out var c) ? c.GetString() ?? "" : "",
                Size = root.TryGetProperty("size", out var s) ? s.GetString() ?? "" : "",
                Gender = root.TryGetProperty("gender", out var g) ? g.GetString() ?? "" : "",
                Material = root.TryGetProperty("material", out var m) ? m.GetString() ?? "" : "",
                Season = root.TryGetProperty("season", out var sea) ? sea.GetString() ?? "" : "",
                Features = features,
                NormalizedQueryRu = root.TryGetProperty("normalizedQueryRu", out var nqRu) ? nqRu.GetString() ?? "" : "",
                NormalizedQueryEn = root.TryGetProperty("normalizedQueryEn", out var nqEn) ? nqEn.GetString() ?? "" : "",
                NormalizedQuery = root.TryGetProperty("normalizedQuery", out var nq) ? nq.GetString() ?? "" : ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing analysis JSON: {Raw}", raw);
            return new AiAnalysis();
        }
    }

    private (AiAnalysis, string) FallbackAnalysis(string query)
    {
        var normalized = query.ToLower().Replace('ё', 'е').Trim();
        var analysis = new AiAnalysis
        {
            NormalizedQueryRu = normalized,
            NormalizedQueryEn = query,
            NormalizedQuery = normalized,
            Features = new List<string>()
        };
        return (analysis, normalized);
    }

    // DTO классы
    private class GigaChatResponse
    {
        [JsonPropertyName("choices")]
        public List<GigaChatChoice>? Choices { get; set; }
    }

    private class GigaChatChoice
    {
        [JsonPropertyName("message")]
        public GigaChatMessage? Message { get; set; }
    }

    private class GigaChatMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class GigaChatTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }
    }
}