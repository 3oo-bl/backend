using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;
using ProfitableViewInfra.Searchers;
using WbGrpc;

namespace ProfitableViewDataInfra.Parsers;

public class OzonMarketplaceParser : IMarketplaceParser
{
    private sealed class ProductJsonContext
    {
        public JsonElement Sticky { get; init; }
        public JsonElement Price { get; init; }
        public JsonElement WebSingleProductScore { get; init; }
        public JsonElement WebGallery { get; init; }
    }
    
    public HttpClient Client { get; }
    public string MarketName { get => "Ozon"; }
    private readonly ILogger<OzonMarketplaceParser> _logger;
    private readonly ISearcher _searcher;

    public OzonMarketplaceParser(ILogger<OzonMarketplaceParser> logger, HttpClient client, OzonSearcher searcher)
    {
        _logger = logger;
        Client = client;
        _searcher = searcher;
    }

    public async Task<List<ProductDTO>> ParseProductList(string itemName, int? quantity)
    {
        if (quantity is null)
            quantity = 20;
        var response = _searcher.Search(itemName, (int)quantity);
        return await ParseResponse(response, quantity);
    }
    
    internal async Task<List<ProductDTO>> ParseResponse(IAsyncEnumerable<SearchResponse> rawData, int? quantity = 20)
    {
        var productsList = new List<ProductDTO>();
        _logger.LogInformation("Парсинг ответа...");
        
        await foreach (var response in rawData)
        {
            if (response.Status != 1)
                continue; // #TODO здесь нужна обработка ошибок парсинга
            var product = JsonSerializer.Deserialize<JsonElement>(response.RawJson);
            try
            {
                productsList.Add(ParseProduct(product));
            }
            catch
            {
                continue;
                //#TODO Убрать это, переделать на Result/более умную обработку данных о товаре (падало на sticky missing)!!
            }
            if (productsList.Count >= quantity)
                break;
        }

        return productsList;
    }

    internal ProductDTO ParseProduct(JsonElement rawProduct)
    {
        var context = ExtractContext(rawProduct);
        
        var rating = ParseRating(context.WebSingleProductScore);

        var id = context.Sticky.GetProperty("sku").GetString()!;
        return new ProductDTO
        {
            Id = id,
            Name = context.Sticky.GetProperty("name").GetString()!,
            Cost = ParseIntNumber(context.Price.GetProperty("price").GetString()!),
            CostWithDiscount = ParseIntNumber(
                context.Price.TryGetProperty("originalPrice", out var originalPrice)
                ? originalPrice.GetString()
                : null),
            Seller = context.Sticky.GetProperty("seller").GetProperty("name").GetString()!,
            Rating = rating.score,
            Reviews = rating.reviewsCount,
            Link = $"https://www.ozon.ru/product/{id}",
            ImageLink = context.WebGallery.GetProperty("coverImage").GetString()!,
        };
    }

    private ProductJsonContext ExtractContext(JsonElement rawProduct)
    {
        JsonElement? stickyProductsInfo = null;
        JsonElement? webPriceInfo = null;
        JsonElement? webSingleProductScore = null;
        JsonElement? webGallery = null;
        foreach (var item in rawProduct.EnumerateArray())
        {
            foreach (var prop in item.EnumerateObject())
            {
                var name = prop.Name;
                var json = prop.Value.GetString();
                if (string.IsNullOrEmpty(json))
                    continue;
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (name.StartsWith("webStickyProducts"))
                    stickyProductsInfo = root.Clone();
                else if (name.StartsWith("webPrice-"))
                    webPriceInfo = root.Clone();
                else if (name.StartsWith("webSingleProductScore"))
                    webSingleProductScore = root.Clone();
                else if (name.StartsWith("webGallery-"))
                    webGallery = root.Clone();
            }
        }
        return new ProductJsonContext
        {
            Sticky = stickyProductsInfo ?? throw new Exception("sticky missing"),
            Price = webPriceInfo ?? throw new Exception("price missing"),
            WebSingleProductScore = webSingleProductScore ?? throw new Exception("score missing"),
            WebGallery = webGallery ?? throw new Exception("web gallery missing"),
        };
    }

    private (float score, int reviewsCount) ParseRating(JsonElement element)
    {
        var text = element.GetProperty("text").GetString() ?? "";
        if (text == "Нет отзывов")
            return (0.0f, 0);
        var parts = text.Split('•', StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            throw new Exception($"Invalid rating format: {text}");
        
        var score = float.Parse(parts[0], CultureInfo.InvariantCulture);
        var reviewsCount = int.Parse(parts[1].Where(char.IsDigit).ToArray());
        return (score, reviewsCount);
    }

    private int ParseIntNumber(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;
        Span<char> buffer = stackalloc char[input.Length];
        var j = 0;
        foreach (var c in input)
        {
            if (char.IsDigit(c))
                buffer[j++] = c;
        }
        return int.Parse(buffer.Slice(0, j));
    }
}