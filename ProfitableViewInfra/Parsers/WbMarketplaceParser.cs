using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;
using ProfitableViewData.Utils;

namespace ProfitableViewData.Parsers;

public class WbMarketplaceParser : IMarketplaceParser
{
    private readonly ILogger<WbMarketplaceParser> _logger;
    public HttpClient Client { get; }
    private readonly ISearcher _searcher;
    public string BaseURL => "https://search.wb.ru/exactmatch/ru/common/v18/search";

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
    
    public bool IsRequested(string market)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ProductDTO>> ParseProductList(string itemName, int? retryCount = 5, float? retryDelay = 2.5f)
    {
        var ans = await _searcher.Search(itemName);
        //var responce = GetResponse(itemName, retryCount);
        return new List<ProductDTO>();
    }

    internal async Task Sleep(int attempt)
    {
        var baseDelay = Math.Min(20, 1.2 * Math.Pow(2, attempt));
        var jitter = Random.Shared.NextDouble() * 2.5 + 0.5;
        await Task.Delay(TimeSpan.FromSeconds(baseDelay + jitter));
    }

    internal async Task E429Handler(HttpResponseMessage response, int currAttempt)
    {
        if (response.Headers.TryGetValues("Retry-After", out var values))
        {
            var wait = int.Parse(values.First());
            _logger.LogInformation($"429, retry later, attempt {currAttempt}, waitTime: {wait}");
            await Task.Delay(TimeSpan.FromSeconds(wait));
        }
        else
        {
            _logger.LogInformation($"429, no retry later, sleeping, attempt {currAttempt}");
            await Sleep(currAttempt);
        }
    }

    internal async Task<JsonElement> GetResponse(string itemName, int? retryCount)
    {
        var encodedQuery = Uri.EscapeDataString(itemName);
        var fullUrl = $"{BaseURL}?appType=1&curr=rub&dest=-1029256&lang=ru&page=1&query={encodedQuery}&resultset=catalog&sort=popular&spp=30";
        await Task.Delay(Random.Shared.Next(1500, 4000));
        
        for (var i = 1; i <= retryCount; ++i)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            request.Headers.Accept.ParseAdd("application/json, text/plain, */*");
            request.Headers.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9");
            request.Headers.Referrer = new Uri("https://www.wildberries.ru/");request.Version = HttpVersion.Version11;request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

            var response = await Client.SendAsync(request);
            Console.WriteLine(response.StatusCode);
            
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await E429Handler(response, i);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                await Sleep(i);
                continue;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
        
            using var doc = await JsonDocument.ParseAsync(stream);
            return doc.RootElement.Clone();
        }

        throw new Exception("Ответа не получено");
        return default; // #TODO Переделать на возврат ошибки??
    }
}