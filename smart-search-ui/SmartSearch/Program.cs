using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qdrant.Client;
using SmartSearch.Data;
using SmartSearch.Infrastructure.Embeddings;
using SmartSearch.Middleware;
using SmartSearch.Services.Ai;
using SmartSearch.Services.Auth;
using SmartSearch.Services.Product;
using SmartSearch.Services.Vector;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
    Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
            npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
        }
    ));

builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartSearch",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SmartSearchUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontend", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ── Qdrant (with in-memory fallback) ─────────────────────────────────────────
builder.Services.AddSingleton<IVectorStore>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<QdrantVectorStore>>();
    var config = sp.GetRequiredService<IConfiguration>();
    var qdrantUrl = Environment.GetEnvironmentVariable("QDRANT_URL")
        ?? config["Qdrant:Url"]
        ?? "http://localhost:6333";

    try
    {
        var uri = new Uri(qdrantUrl);

        var client = new QdrantClient(host: uri.Host, port: uri.Port, https: uri.Scheme == "https");

        client.ListCollectionsAsync().GetAwaiter().GetResult();

        logger.LogInformation("Connected to Qdrant at {Url}", qdrantUrl);
        return new QdrantVectorStore(client, config, logger);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Qdrant unavailable, using in-memory store");
        return new InMemoryVectorStore();
    }
});

// ── Embedding Provider ────────────────────────────────────────────────────────
builder.Services.AddSingleton<IEmbeddingProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var clientId = config["GigaChat:ClientId"];
    var clientSecret = config["GigaChat:ClientSecret"];

    if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
    {
        sp.GetRequiredService<ILogger<Program>>().LogInformation("Using GigaChat embedding provider");
        return sp.GetRequiredService<GigaChatEmbeddingProvider>();
    }

    sp.GetRequiredService<ILogger<Program>>().LogWarning("GigaChat credentials missing, using simple embedding provider (poor quality)");
    return new SimpleEmbeddingProvider();
});
builder.Services.AddSingleton<IEmbeddingProvider, SimpleEmbeddingProvider>();

// ── HTTP Clients ──────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("GigaChat")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddHttpClient("GigaChatAuth")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddSingleton<GigaChatService>();
builder.Services.AddScoped<AiSearchService>();
builder.Services.AddSingleton<CsvProductLoader>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("VueFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("Migration failed: {Message}", ex.Message);
    }
}

app.Run();
