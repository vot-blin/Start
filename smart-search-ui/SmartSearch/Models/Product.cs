namespace SmartSearch.Models;

public class Product
{
    public int Id { get; set; }                    
    public string Title { get; set; } = string.Empty;          
    public string Url { get; set; } = string.Empty;             
    public string Pid { get; set; } = string.Empty;             
    public string FormattedUrl { get; set; } = string.Empty;    
    public decimal Price { get; set; }                          
    public string Currency { get; set; } = string.Empty;        
    public decimal? OriginalPrice { get; set; }                  
    public string Discount { get; set; } = string.Empty;        
    public bool FAssured { get; set; }                          
    public string Highlights { get; set; } = string.Empty;      
    public string Seller { get; set; } = string.Empty;          
    public double? SellerRating { get; set; }                    
    public string Description { get; set; } = string.Empty;     
    public string Specifications { get; set; } = string.Empty;  
    public string FormattedSpecifications { get; set; } = string.Empty; 
    public string Images { get; set; } = string.Empty;         
    public string ReturnPolicy { get; set; } = string.Empty;    
    public string Category { get; set; } = string.Empty;        
    public string SubCategory1 { get; set; } = string.Empty;   
    public string SubCategory2 { get; set; } = string.Empty;    
    public string SubCategory3 { get; set; } = string.Empty;    
    public string Breadcrumbs { get; set; } = string.Empty;     
    public string CountryOfOrigin { get; set; } = string.Empty; 
    public string GenericName { get; set; } = string.Empty;     
    public string ManufacturedBy { get; set; } = string.Empty;  
    public double? AvgRatings { get; set; }                      
    public int ReviewsCount { get; set; }                       
    public int RatingsCount { get; set; }                       
    public string UniqId { get; set; } = string.Empty;          
    public DateTime ScrapedAt { get; set; }
}
