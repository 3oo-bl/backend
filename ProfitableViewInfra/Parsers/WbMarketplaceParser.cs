using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.Searchers;
using WbGrpc;

namespace ProfitableViewDataInfra.Parsers;

public class WbMarketplaceParser : IMarketplaceParser
{
    private readonly ILogger<WbMarketplaceParser> _logger;
    public HttpClient Client { get; }
    private readonly ISearcher _searcher;

    public string MarketName
    {
        get => "Wildberries";
    }

    public WbMarketplaceParser(ILogger<WbMarketplaceParser> logger, HttpClient client, WbSearcher searcher)
    {
        _logger = logger;
        Client = client;
        Client.DefaultRequestHeaders.ConnectionClose = false;
        Client.DefaultRequestHeaders.ExpectContinue = false;
        _searcher = searcher;
    }

    public async Task<List<ProductDTO>> ParseProductList(string itemName, int? quantity = 100)
    {
        var result = new List<ProductDTO>();
        var pages = (quantity + 99) / 100;
        for (var i = 1; i <= pages; ++i)
        {
            var response = _searcher.Search(itemName, i);
            List<ProductDTO> parsed;
            if (i * 100 > quantity)
                parsed = await ParseResponse(response, quantity % 100);
            else
                parsed = await ParseResponse(response);
            lock (result)
                result.AddRange(parsed);
        }

        return result;
    }

    internal async Task<List<ProductDTO>> ParseResponse(IAsyncEnumerable<SearchResponse> rawData, int? quantity = 100)
    {
        var productsList = new List<ProductDTO>();

        await foreach (var response in rawData)
        {
            if (response.Status != 1)
                continue; // #TODO здесь нужна обработка ошибок парсинга
            var productWrapper = JsonSerializer.Deserialize<WbProductWrapper>(response.RawJson)!;
            foreach (var product in productWrapper.Products)
            {
                productsList.Add(ParseProduct(product));
                if (productsList.Count >= quantity)
                    break;
            }
        }
        _logger.LogInformation($"Parsed {productsList.Count} products");

        return productsList;
    }

    internal ProductDTO ParseProduct(WbProductDTO rawProduct)
    {
        var priceInfo = rawProduct.Sizes[0].WbPrice;
        var id = rawProduct.Id;
        var productDto = new ProductDTO
        {
            Id = id.ToString(),
            Name = rawProduct.Name,
            Cost = priceInfo.Product / 100,
            CostWithDiscount = priceInfo.Basic / 100,
            Subcategory = null, //#TODO Разобраться, что это и где можно взять
            Category = null, //#TODO Разобраться, что это и где можно взять
            Cashback = priceInfo.Return / 100,
            Brand = rawProduct.Brand,
            Seller = rawProduct.Supplier,
            SellerRating = (float)rawProduct.SupplierRating,
            Rating = (float)rawProduct.ReviewRating,
            Reviews = rawProduct.Feedbacks,
            Remaining = rawProduct.TotalQuantity,
            Link = $"https://www.wildberries.ru/catalog/{rawProduct.Id}/detail.aspx",
            ImageLink = $"https://ekt-basket-cdn-06bl.geobasket.ru/vol{id / 100000}/part{id / 1000}/{id}/images/big/1.webp"
        };
        return productDto;
    }
}