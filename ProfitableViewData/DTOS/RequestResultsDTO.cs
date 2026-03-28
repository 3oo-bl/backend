namespace ProfitableViewData.DTOS;

public class RequestResultsDTO
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public string? OrderBy { get; set; }
}