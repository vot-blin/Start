namespace SmartSearch.Infrastructure.Embeddings;

public interface IEmbeddingProvider
{
    Task<float[]> GetEmbeddingAsync(string text);
    Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts);
}
