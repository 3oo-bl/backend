namespace ProfitableViewApp.DTOS;

public class RequestStartDTO
{
    public string Item { get; set; }
    public string[] Markets { get; set; }
    public int? RetryCount { get; set; }
    public float? RetryDelay { get; set; }
}