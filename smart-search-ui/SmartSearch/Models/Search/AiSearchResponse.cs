using SmartSearch.Models;

namespace SmartSearch.Models.Search;

public class AiSearchResponse
{
    public List<Product> Products { get; set; } = new();
    public AiAnalysis? Analysis { get; set; }
}

public class AiAnalysis
{
    public string ProductType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public string NormalizedQueryRu { get; set; } = string.Empty;
    public string NormalizedQueryEn { get; set; } = string.Empty;
    public string NormalizedQuery { get; set; } = string.Empty;
}
