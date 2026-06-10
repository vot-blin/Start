using Microsoft.EntityFrameworkCore;
using SmartSearch.Data;
using SmartSearch.Models;

namespace SmartSearch.Services.Product;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<List<Models.Product>> GetAllAsync(int page = 1, int pageSize = 100)
    {
        return await _db.Products
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Models.Product?> GetByIdAsync(int id)
    {
        return await _db.Products.FindAsync(id);
    }

    public async Task<Models.Product?> GetByUniqIdAsync(string uniqId)
    {
        return await _db.Products.FirstOrDefaultAsync(p => p.UniqId == uniqId);
    }

    public async Task<List<Models.Product>> SearchAsync(
        string? query,
        string? category = null,
        string? subCategory = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int limit = 200)
    {
        var q = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.ToLower();
            q = q.Where(p =>
                EF.Functions.ILike(p.Title, $"%{lower}%") ||
                EF.Functions.ILike(p.Description, $"%{lower}%") ||
                EF.Functions.ILike(p.Category, $"%{lower}%") ||
                EF.Functions.ILike(p.SubCategory1, $"%{lower}%") ||
                EF.Functions.ILike(p.SubCategory2, $"%{lower}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(p => EF.Functions.ILike(p.Category, $"%{category}%"));

        if (!string.IsNullOrWhiteSpace(subCategory))
            q = q.Where(p =>
                EF.Functions.ILike(p.SubCategory1, $"%{subCategory}%") ||
                EF.Functions.ILike(p.SubCategory2, $"%{subCategory}%") ||
                EF.Functions.ILike(p.SubCategory3, $"%{subCategory}%"));

        if (minPrice.HasValue)
            q = q.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            q = q.Where(p => p.Price <= maxPrice.Value);

        return await q
            .OrderBy(p => p.Id)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _db.Products
            .Select(p => p.Category)
            .Where(c => c != null && c != string.Empty)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<string>>> GetCategoriesWithSubCategoriesAsync()
    {
        var products = await _db.Products
            .Where(p => p.Category != null && p.Category != string.Empty)
            .Select(p => new { p.Category, p.SubCategory1, p.SubCategory2 })
            .Distinct()
            .ToListAsync();

        var result = new Dictionary<string, List<string>>();

        foreach (var product in products)
        {
            if (!result.ContainsKey(product.Category))
                result[product.Category] = new List<string>();

            if (!string.IsNullOrWhiteSpace(product.SubCategory1) &&
                !result[product.Category].Contains(product.SubCategory1))
                result[product.Category].Add(product.SubCategory1);

            if (!string.IsNullOrWhiteSpace(product.SubCategory2) &&
                !result[product.Category].Contains(product.SubCategory2))
                result[product.Category].Add(product.SubCategory2);
        }

        foreach (var key in result.Keys.ToList())
            result[key] = result[key].OrderBy(x => x).ToList();

        return result;
    }

    public async Task UpsertAsync(IEnumerable<Models.Product> products)
    {
        // Полностью очищаем таблицу
        _db.Products.RemoveRange(_db.Products);
        await _db.SaveChangesAsync();

        // Добавляем товары без Id из CSV
        foreach (var product in products)
        {
            var newProduct = new Models.Product
            {
                Title = product.Title,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                Images = product.Images,
                Url = product.Url,
                Pid = product.Pid,
                FormattedUrl = product.FormattedUrl,
                Currency = product.Currency,
                OriginalPrice = product.OriginalPrice,
                Discount = product.Discount,
                FAssured = product.FAssured,
                Highlights = product.Highlights,
                Seller = product.Seller,
                SellerRating = product.SellerRating,
                Specifications = product.Specifications,
                FormattedSpecifications = product.FormattedSpecifications,
                ReturnPolicy = product.ReturnPolicy,
                SubCategory1 = product.SubCategory1,
                SubCategory2 = product.SubCategory2,
                SubCategory3 = product.SubCategory3,
                Breadcrumbs = product.Breadcrumbs,
                CountryOfOrigin = product.CountryOfOrigin,
                GenericName = product.GenericName,
                ManufacturedBy = product.ManufacturedBy,
                AvgRatings = product.AvgRatings,
                ReviewsCount = product.ReviewsCount,
                RatingsCount = product.RatingsCount,
                UniqId = $"{product.UniqId}_{Guid.NewGuid()}", 
                ScrapedAt = product.ScrapedAt
            };

            await _db.Products.AddAsync(newProduct);
        }

        await _db.SaveChangesAsync();
    }
    public async Task<int> GetCountAsync()
    {
        return await _db.Products.CountAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
            return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAllAsync()
    {
        _db.Products.RemoveRange(_db.Products);
        await _db.SaveChangesAsync();
    }
}