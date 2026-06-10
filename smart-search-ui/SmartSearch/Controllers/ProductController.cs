using Microsoft.AspNetCore.Mvc;
using SmartSearch.Services.Product;

namespace SmartSearch.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _products;

    public ProductController(ProductService products) => _products = products;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var products = await _products.GetAllAsync(page, pageSize);
        return Ok(new { products });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound(new { message = "Товар не найден" });
        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] string? subCategory = null,  // ← добавлено
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
             [FromQuery] int limit = 200)
    {
        // ✅ Правильный порядок параметров
        var products = await _products.SearchAsync(
            query: q,
            category: category,
            subCategory: subCategory,
            minPrice: minPrice,
            maxPrice: maxPrice,
            limit: limit);

        return Ok(new { products, total = products.Count, limit });
    }
    
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _products.GetCategoriesAsync();
        return Ok(new { categories });
    }
}
