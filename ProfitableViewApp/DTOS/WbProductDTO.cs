using System.Text.Json.Serialization;

namespace ProfitableViewApp.DTOS;

public class WbProductDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("sizes")]
    public List<SizesDTO> Sizes { get; set; }
    
    [JsonPropertyName("brand")]
    public string Brand { get; set; }
    
    [JsonPropertyName("supplier")]
    public string Supplier { get; set; }
    
    [JsonPropertyName("supplierRating")]
    public double SupplierRating { get; set; }
    
    [JsonPropertyName("reviewRating")]
    public double ReviewRating { get; set; }
    
    [JsonPropertyName("feedbacks")]
    public int Feedbacks { get; set; }
    
    [JsonPropertyName("totalQuantity")]
    public int TotalQuantity { get; set; }
}

public class SizesDTO
{
    [JsonPropertyName("price")]
    public PriceDTO Price { get; set; }
}

public class PriceDTO
{
    [JsonPropertyName("basic")]
    public int Basic { get; set; }
    
    [JsonPropertyName("product")]
    public int Product { get; set; }
    
    [JsonPropertyName("return")]
    public int Return { get; set; }
}

public class WbProductWrapper
{
    [JsonPropertyName("products")]
    public List<WbProductDTO> Products { get; set; }
}