using Microsoft.AspNetCore.Mvc;
using SmartSearch.Infrastructure.Embeddings;
using SmartSearch.Services.Product;
using SmartSearch.Services.Vector;

namespace SmartSearch.Controllers;

[ApiController]
[Route("api/vector")]
public class VectorController : ControllerBase
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingProvider _embeddings;
    private readonly CsvProductLoader _csvLoader;
    private readonly ProductService _products;
    private readonly ILogger<VectorController> _logger;

    public VectorController(
        IVectorStore vectorStore,
        IEmbeddingProvider embeddings,
        CsvProductLoader csvLoader,
        ProductService products,
        ILogger<VectorController> logger)
    {
        _vectorStore = vectorStore;
        _embeddings = embeddings;
        _csvLoader = csvLoader;
        _products = products;
        _logger = logger;
    }

    [HttpPost("reindex")]
    public async Task<IActionResult> Reindex()
    {
        try
        {
            var productList = _csvLoader.Load();
            if (productList.Count == 0)
                return BadRequest(new { message = "CSV файл не найден или пуст" });

            // Persist to PostgreSQL
            await _products.UpsertAsync(productList);

            // Generate embeddings and index in vector store
            await _vectorStore.ClearAsync();
            var texts = productList.Select(p => $"{p.Title} {p.Category} {p.Description}");
            var vectors = await _embeddings.GetEmbeddingsAsync(texts);

            var indexed = productList.Zip(vectors, (p, v) => (p, v)).ToList();
            await _vectorStore.IndexProductsAsync(indexed);

            _logger.LogInformation("Reindexed {Count} products", productList.Count);
            return Ok(new { message = $"Проиндексировано {productList.Count} товаров", count = productList.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reindex failed");
            return StatusCode(500, new { message = "Ошибка при индексации", detail = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Параметр q обязателен" });

        var vector = await _embeddings.GetEmbeddingAsync(q);
        var results = await _vectorStore.SearchAsync(vector, limit);
        return Ok(new { products = results });
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var healthy = await _vectorStore.IsHealthyAsync();
        var storeType = _vectorStore.GetType().Name;
        return Ok(new { healthy, storeType });
    }
}
