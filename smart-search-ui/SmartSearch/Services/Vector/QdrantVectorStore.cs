using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartSearch.Models;

namespace SmartSearch.Services.Vector;

public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly string _collection;
    private readonly int _vectorSize;
    private readonly ILogger<QdrantVectorStore> _logger;

    public QdrantVectorStore(QdrantClient client, IConfiguration config, ILogger<QdrantVectorStore> logger)
    {
        _client = client;
        _collection = config["Qdrant:CollectionName"] ?? "products";
        _vectorSize = int.Parse(config["Qdrant:VectorSize"] ?? "384");
        _logger = logger;
    }

    public async Task IndexProductsAsync(IEnumerable<(SmartSearch.Models.Product product, float[] vector)> items)
    {
        await EnsureCollectionAsync();

        var points = items.Select(item => new PointStruct
        {
            Id = (ulong)item.product.Id,
            Vectors = item.vector,
            Payload =
            {
            ["title"] = item.product.Title,
            ["description"] = item.product.Description,
            ["category"] = item.product.Category,
            ["sub_category_1"] = item.product.SubCategory1 ?? "",
            ["sub_category_2"] = item.product.SubCategory2 ?? "",
            ["price"] = (double)item.product.Price,
            ["images"] = item.product.Images ?? "", 
            ["seller"] = item.product.Seller ?? "",
            ["avg_ratings"] = item.product.AvgRatings ?? 0,
            ["uniq_id"] = item.product.UniqId ?? "",
            ["currency"] = item.product.Currency ?? "INR",
            }
        }).ToList();

        if (points.Count == 0) return;

        const int batchSize = 100;
        for (int i = 0; i < points.Count; i += batchSize)
        {
            var batch = points.Skip(i).Take(batchSize).ToList();
            await _client.UpsertAsync(_collection, batch);
        }

        _logger.LogInformation("Indexed {Count} products in Qdrant collection '{Collection}'", points.Count, _collection);
    }

    public async Task<List<SmartSearch.Models.Product>> SearchAsync(float[] queryVector, int limit = 10)
    {
        var searchResult = await _client.SearchAsync(_collection, queryVector, limit: (ulong)limit);

        var products = searchResult.Select(r => new SmartSearch.Models.Product
        {
            Id = (int)r.Id.Num,
            Title = GetPayloadString(r.Payload, "title"),
            Description = GetPayloadString(r.Payload, "description"),
            Category = GetPayloadString(r.Payload, "category"),
            Price = (decimal)GetPayloadDouble(r.Payload, "price"),
            Images = GetPayloadString(r.Payload, "images"), 
            Seller = GetPayloadString(r.Payload, "seller"),
            AvgRatings = GetPayloadDouble(r.Payload, "avg_ratings"),
            UniqId = GetPayloadString(r.Payload, "uniq_id"),
            Currency = "INR",
            Url = "",
            Pid = "",
            FormattedUrl = "",
            OriginalPrice = null,
            Discount = "",
            FAssured = false,
            Highlights = "",
            SellerRating = null,
            Specifications = "",
            FormattedSpecifications = "",
            ReturnPolicy = "",
            SubCategory1 = "",
            SubCategory2 = "",
            SubCategory3 = "",
            Breadcrumbs = "",
            CountryOfOrigin = "",
            GenericName = "",
            ManufacturedBy = "",
            ReviewsCount = 0,
            RatingsCount = 0,
            ScrapedAt = DateTime.UtcNow
        }).ToList();

        _logger.LogInformation("Search returned {Count} products, first images: {Images}",
            products.Count, products.FirstOrDefault()?.Images);

        return products;
    }

    public async Task<List<(SmartSearch.Models.Product product, float score)>> SearchWithScoreAsync(float[] queryVector, int limit = 10)
    {
        var searchResult = await _client.SearchAsync(_collection, queryVector, limit: (ulong)limit);

        var results = new List<(SmartSearch.Models.Product product, float score)>();

        foreach (var r in searchResult)
        {
            var product = new SmartSearch.Models.Product
            {
                Id = (int)r.Id.Num,
                Title = GetPayloadString(r.Payload, "title"),
                Description = GetPayloadString(r.Payload, "description"),
                Category = GetPayloadString(r.Payload, "category"),
                SubCategory1 = GetPayloadString(r.Payload, "sub_category_1"),
                SubCategory2 = GetPayloadString(r.Payload, "sub_category_2"),
                Price = (decimal)GetPayloadDouble(r.Payload, "price"),
                Images = GetPayloadString(r.Payload, "images"),
                Seller = GetPayloadString(r.Payload, "seller"),
                AvgRatings = GetPayloadDouble(r.Payload, "avg_ratings"),
                UniqId = GetPayloadString(r.Payload, "uniq_id")
            };

            results.Add((product, (float)r.Score));
        }

        return results;
    }

    public async Task ClearAsync()
    {
        var exists = await CollectionExistsAsync();
        if (exists)
        {
            await _client.DeleteCollectionAsync(_collection);
            _logger.LogInformation("Deleted Qdrant collection '{Collection}'", _collection);
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var collections = await _client.ListCollectionsAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<long> GetPointsCountAsync()
    {
        try
        {
            var collectionInfo = await _client.GetCollectionInfoAsync(_collection);
            return (long)collectionInfo.PointsCount; // ✅ Явное преобразование ulong в long
        }
        catch
        {
            return 0;
        }
    }

    private async Task EnsureCollectionAsync()
    {
        if (!await CollectionExistsAsync())
        {
            await _client.CreateCollectionAsync(_collection, new VectorParams
            {
                Size = (ulong)_vectorSize,
                Distance = Distance.Cosine
            });
            _logger.LogInformation("Created Qdrant collection '{Collection}' with vector size {Size}", _collection, _vectorSize);
        }
    }

    private async Task<bool> CollectionExistsAsync()
    {
        try
        {
            var collections = await _client.ListCollectionsAsync();
            return collections.Any(c => c == _collection);
        }
        catch
        {
            return false;
        }
    }

    // Вспомогательные методы для безопасного извлечения данных из Payload
    private static string GetPayloadString(Google.Protobuf.Collections.MapField<string, Qdrant.Client.Grpc.Value> payload, string key)
    {
        return payload.TryGetValue(key, out var value) ? value.StringValue : "";
    }

    private static double GetPayloadDouble(Google.Protobuf.Collections.MapField<string, Qdrant.Client.Grpc.Value> payload, string key)
    {
        return payload.TryGetValue(key, out var value) ? value.DoubleValue : 0.0;
    }
}