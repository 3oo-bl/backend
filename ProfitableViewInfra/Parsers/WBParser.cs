using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;

namespace ProfitableViewData.Parsers;

public class WBParser : IParser
{
    private readonly ILogger<WBParser> _logger;

    public HttpClient Client { get; }

    public WBParser(ILogger<WBParser> logger, HttpClient client)
    {
        _logger = logger;
        Client = client;
    }

    public string MarketName
    {
        get => "Wildberries";
    }
    
    public bool IsRequested(string market)
    {
        throw new NotImplementedException();
    }

    public Task<List<ProductDTO>> ParseProductList(string itemName, int? retryCount = 1, float? retryDelay = 0)
    {
        var catalog = GetCatalog();
        throw new NotImplementedException();
    }

    internal async Task<JsonElement> GetCatalog()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://static-basket-01.wbbasket.ru/vol0/data/main-menu-ru-ru-v3.json");
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        
        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        await using var stream = await response.Content.ReadAsStreamAsync();
        
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.Clone();
    }

    internal async Task GetCategory()
    {
        
    }
}