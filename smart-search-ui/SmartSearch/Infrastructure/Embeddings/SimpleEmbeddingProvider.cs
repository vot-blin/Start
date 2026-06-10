namespace SmartSearch.Infrastructure.Embeddings;

public class SimpleEmbeddingProvider : IEmbeddingProvider
{
    private readonly Random _random = new();
    private readonly int _vectorSize = 384;

    public Task<float[]> GetEmbeddingAsync(string text)
    {
        var vector = new float[_vectorSize];
        for (int i = 0; i < _vectorSize; i++)
            vector[i] = (float)_random.NextDouble();

        return Task.FromResult(vector);
    }

    public Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts)
    {
        var vectors = texts.Select(_ =>
        {
            var vector = new float[_vectorSize];
            for (int i = 0; i < _vectorSize; i++)
                vector[i] = (float)_random.NextDouble();
            return vector;
        }).ToList();

        return Task.FromResult(vectors);
    }
}