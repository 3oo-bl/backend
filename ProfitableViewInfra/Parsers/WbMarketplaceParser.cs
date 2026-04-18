using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.Utils;

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

    public WbMarketplaceParser(ILogger<WbMarketplaceParser> logger, HttpClient client, ISearcher searcher)
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
            var response = await _searcher.Search(itemName, i);
            if (string.IsNullOrEmpty(response))
                continue;
            List<ProductDTO> parsed;
            if (i * 100 >= quantity)
                parsed = ParseResponse(response, quantity % 100).Result;
            else
                parsed = ParseResponse(response).Result;
            lock (result)
                result.AddRange(parsed);
        }

        return result;
    }

    internal async Task<List<ProductDTO>> ParseResponse(string response, int? quantity = 100)
    {
        var clearedData = JsonSerializer.Deserialize<WbProductWrapper>(response);
        var productsList = new List<ProductDTO>();
        var i = 0;

        foreach (var product in clearedData.Products)
        {
            productsList.Add(ParseProduct(product).Result);
            ++i;
            if (i >= quantity)
                break;
        }

        return productsList;
    }

    internal async Task<ProductDTO> ParseProduct(WbProductDTO rawProduct)
    {
        var priceInfo = rawProduct.Sizes[0].Price;
        var productDTO = new ProductDTO
        {
            Id = rawProduct.Id.ToString(),
            Name = rawProduct.Name,
            Cost = priceInfo.Basic / 100,
            CostWithDiscount =priceInfo.Product / 100,
            Subcategory = null, //#TODO Разобраться, что это и где можно взять
            Category = null, //#TODO Разобраться, что это и где можно взять
            Cashback = priceInfo.Return / 100,
            Brand = rawProduct.Brand,
            Seller = rawProduct.Supplier,
            SellerRating = (float)rawProduct.SupplierRating,
            Rating = (float)rawProduct.ReviewRating,
            Reviews = rawProduct.Feedbacks,
            Remaining = rawProduct.TotalQuantity,
            Link = $"https://www.wildberries.ru/catalog/{rawProduct.Id}/detail.aspx"
        };
        return productDTO;
    }
}