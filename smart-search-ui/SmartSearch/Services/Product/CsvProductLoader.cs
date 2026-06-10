using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using SmartSearch.Models;

namespace SmartSearch.Services.Product;

public class CsvProductLoader
{
    private readonly IConfiguration _config;
    private readonly ILogger<CsvProductLoader> _logger;

    public CsvProductLoader(IConfiguration config, ILogger<CsvProductLoader> logger)
    {
        _config = config;
        _logger = logger;
    }

    public List<Models.Product> Load(string? customPath = null)
    {
        var path = customPath
            ?? _config["Products:CsvPath"]
            ?? "wwwroot/data/fashion_clothing_and_accessories_products_dataset_sample.csv";

        if (!File.Exists(path))
        {
            _logger.LogWarning("Products CSV not found at {Path}", path);
            return new List<Models.Product>();
        }

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            Delimiter = ",",
            TrimOptions = TrimOptions.Trim
        };

        try
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, csvConfig);

            csv.Context.RegisterClassMap<ProductCsvMap>();

            var products = csv.GetRecords<Models.Product>().ToList();
            _logger.LogInformation("Loaded {Count} products from CSV", products.Count);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CSV from {Path}", path);
            return new List<Models.Product>();
        }
    }
}

public class ProductCsvMap : ClassMap<Models.Product>
{
    public ProductCsvMap()
    {
        // Индекс (первая колонка без имени)
        //Map(p => p.Id).Index(0);

        // Основные поля из CSV (по именам колонок)
        Map(p => p.Title).Name("title");
        Map(p => p.Url).Name("url");
        Map(p => p.Pid).Name("pid");
        Map(p => p.FormattedUrl).Name("formatted_url");
        Map(p => p.Price).Name("price").TypeConverter<PriceConverter>();
        Map(p => p.Currency).Name("currency");

        // Исправленный конвертер для original_price (удаляем символы валюты)
        Map(p => p.OriginalPrice).Name("original_price").TypeConverter<PriceConverter>();

        Map(p => p.Discount).Name("discount");
        Map(p => p.FAssured).Name("f_assured").TypeConverter<BooleanConverter>();
        Map(p => p.Highlights).Name("highlights");
        Map(p => p.Seller).Name("seller");
        Map(p => p.SellerRating).Name("seller_rating").TypeConverter<DoubleConverter>();
        Map(p => p.Description).Name("description");
        Map(p => p.Specifications).Name("specifications");
        Map(p => p.FormattedSpecifications).Name("formatted_specifications");
        Map(p => p.Images).Name("images");
        Map(p => p.ReturnPolicy).Name("return_policy");
        Map(p => p.Category).Name("category");
        Map(p => p.SubCategory1).Name("sub_category_1");
        Map(p => p.SubCategory2).Name("sub_category_2");
        Map(p => p.SubCategory3).Name("sub_category_3");
        Map(p => p.Breadcrumbs).Name("breadcrumbs");
        Map(p => p.CountryOfOrigin).Name("country_of_origin");
        Map(p => p.GenericName).Name("generic_name");
        Map(p => p.ManufacturedBy).Name("manufactured_by");
        Map(p => p.AvgRatings).Name("avg_ratings").TypeConverter<DoubleConverter>();
        Map(p => p.ReviewsCount).Name("reviews_count").TypeConverter<Int32Converter>();
        Map(p => p.RatingsCount).Name("ratings_count").TypeConverter<Int32Converter>();
        Map(p => p.UniqId).Name("uniq_id");
        Map(p => p.ScrapedAt).Name("scraped_at").TypeConverter<DateTimeConverter>();
    }
}

// Кастомный конвертер для цен (удаляет символы валюты)
public class PriceConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var cleaned = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());

        cleaned = cleaned.Replace(',', '.');

        if (cleaned.Count(c => c == '.') > 1)
        {
            var parts = cleaned.Split('.');
            cleaned = parts[0] + "." + string.Join("", parts.Skip(1));
        }

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        return value?.ToString() ?? "";
    }
}

// Кастомный конвертер для double
public class DoubleConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        return value?.ToString() ?? "";
    }
}

// Кастомный конвертер для boolean
public class BooleanConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim().ToLower();
        return text == "true" || text == "1" || text == "yes" || text == "on" || text == "true";
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        return value?.ToString() ?? "false";
    }
}