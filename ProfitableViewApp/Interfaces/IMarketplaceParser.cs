using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;

namespace ProfitableViewApp.Interfaces;

public interface IMarketplaceParser
{
    public string MarketName { get; }
    public Task<List<ProductDTO>> ParseProductList(string itemName, int? quantity = 100);
}