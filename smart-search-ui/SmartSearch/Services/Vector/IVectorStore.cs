using SmartSearch.Models;

namespace SmartSearch.Services.Vector;

public interface IVectorStore
{
    Task IndexProductsAsync(IEnumerable<(SmartSearch.Models.Product product, float[] vector)> items);
    Task<List<SmartSearch.Models.Product>> SearchAsync(float[] queryVector, int limit = 10);
    Task<List<(SmartSearch.Models.Product product, float score)>> SearchWithScoreAsync(float[] queryVector, int limit = 10);
    Task ClearAsync();
    Task<bool> IsHealthyAsync();
    Task<long> GetPointsCountAsync();
}