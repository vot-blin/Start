using System.Text;
using System.Text.Json;

namespace SmartSearch.Infrastructure.Embeddings;

public class GigaChatEmbeddingProvider : IEmbeddingProvider
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<GigaChatEmbeddingProvider> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public GigaChatEmbeddingProvider(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<GigaChatEmbeddingProvider> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        await EnsureAuthenticatedAsync();

        var client = _httpFactory.CreateClient("GigaChat");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        var request = new
        {
            texts = new[] { text },
            model = "embeddings"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://gigachat.devices.sberbank.ru/api/v1/embeddings", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Embedding API failed: {StatusCode}", response.StatusCode);
            return new float[384];
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(json);

        return result?.Embeddings?.FirstOrDefault()?.Embedding?.Select(x => (float)x).ToArray() ?? new float[384];
    }

    public async Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts)
    {
        await EnsureAuthenticatedAsync();

        var client = _httpFactory.CreateClient("GigaChat");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        var request = new
        {
            texts = texts.ToArray(),
            model = "embeddings"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://gigachat.devices.sberbank.ru/api/v1/embeddings", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Embedding API failed: {StatusCode}", response.StatusCode);
            return texts.Select(_ => new float[384]).ToList();
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(json);

        return result?.Embeddings?.Select(e => e.Embedding?.Select(x => (float)x).ToArray() ?? new float[384]).ToList()
            ?? texts.Select(_ => new float[384]).ToList();
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            return;

        var clientId = _config["GigaChat:ClientId"];
        var clientSecret = _config["GigaChat:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogWarning("GigaChat credentials not configured");
            return;
        }

        var authClient = _httpFactory.CreateClient("GigaChatAuth");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        authClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        authClient.DefaultRequestHeaders.Add("RqUID", Guid.NewGuid().ToString());

        var authUrl = _config["GigaChat:AuthUrl"] ?? "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
        var scope = _config["GigaChat:Scope"] ?? "GIGACHAT_API_PERS";

        var formData = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("scope", scope) });
        var response = await authClient.PostAsync(authUrl, formData);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GigaChat auth failed: {StatusCode}", response.StatusCode);
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

        _accessToken = tokenResponse?.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse?.ExpiresIn ?? 3600);
    }

    private class EmbeddingResponse
    {
        public List<EmbeddingData> Embeddings { get; set; } = new();
    }

    private class EmbeddingData
    {
        public List<double> Embedding { get; set; } = new();
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = "";
        public int ExpiresIn { get; set; }
    }
}