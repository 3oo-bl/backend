using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;

namespace ProfitableViewApp.Interfaces;

public interface IParser
{
    public HttpClient Client { get; }
    public string MarketName { get; }
    public bool IsRequested(string market);
    public Task<List<ProductDTO>> ParseProductList(string itemName, int? retryCount = 1, float? retryDelay = 0f);
}