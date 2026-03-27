namespace ProfitableViewData.DTOS;

public class ProductDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public int CostWithDiscount { get; set; }
    public string Subcategory { get; set; }
    public string Category { get; set; }
    public int? Cashback { get; set; }
    public string Brand { get; set; }
    public string Seller { get; set; }
    public float SellerRating { get; set; }
    public float Rating { get; set; }
    public int Reviews { get; set; }
    public int Remaining { get; set; }
    public string Link { get; set; }
}