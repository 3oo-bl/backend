using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;

namespace ProfitableViewDataInfra.Parsers;

public class OzonMarketplaceParser : IMarketplaceParser
{
    public HttpClient Client { get; }
    public string MarketName { get; }
    private readonly ILogger<OzonMarketplaceParser> _logger;
    private readonly ISearcher _searcher;

    public OzonMarketplaceParser(ILogger<OzonMarketplaceParser> logger, HttpClient client, ISearcher searcher)
    {
        _logger = logger;
        Client = client;
        _searcher = searcher;
    }

    public async Task<List<ProductDTO>> ParseProductList(string itemName, int? quantity)
    {
        var results = new List<ProductDTO>();
        if (quantity is null)
            quantity = 20;
        var responce = await _searcher.Search(itemName, (int)quantity);
        File.WriteAllText(Environment.CurrentDirectory + "/ozonProducts.json", responce);
        return results;
    }
}