namespace ProfitableViewApp.DTOS;

public class JobResult
{
    public string Token { get; set; }
    public ParsingJobStates  State { get; set; }
    public List<ProductDTO>? Products { get; set; }
    public Exception? Exception { get; set; }
}