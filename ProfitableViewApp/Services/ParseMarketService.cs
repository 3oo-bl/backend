using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;

namespace ProfitableViewApp.Services;

public class ParseMarketService
{
    private readonly Dictionary<string, IMarketplaceParser> _parsers;
    private readonly ILogger _logger;
    private readonly IPollingService _pollingService;
    
    public ParseMarketService(IEnumerable<IMarketplaceParser> parsers, ILogger logger, IPollingService pollingService)
    {
        _parsers = new Dictionary<string, IMarketplaceParser>();
        _logger = logger;
        foreach (var parser in parsers)
        {
            if (_parsers.ContainsKey(parser.MarketName))
            {
                throw new InvalidOperationException($"Сервис {parser.MarketName} пытался добавиться дважды!");
            }

            _logger.LogInformation($"Зарегистрирован сервис {parser.MarketName}");

            _parsers.Add(parser.MarketName, parser);
        }
        _pollingService = pollingService;
    }

    public string ParseProductList(RequestStartItem request)
    {
        var markets = request.Markets.Length > 0
            ? request.Markets
            : _parsers.Keys.ToArray();
        
        var requestToken = Guid.NewGuid().ToString();
        var jobTokens = new List<string>();
        var tasks = new Dictionary<string, Task<List<ProductDTO>>>();
        foreach (var market in markets)
        {
            var token = GetHash(request.Item + market);
            jobTokens.Add(token);
            if (!_pollingService.AddJob(token))
            {
                continue;
            }
            if (_parsers.TryGetValue(market, out IMarketplaceParser parser))
                tasks[token] = parser.ParseProductList(request.Item, request.Quantity);
            else
                _logger.LogWarning($"Парсера для магазина {market} не существует");
        }
        _pollingService.AddRequest(requestToken, jobTokens);

        if (tasks.Count > 0)
            _ = Task.Run(() => RunJobAsync(tasks));
        
        return requestToken;
    }

    private string GetHash(string input)
    {
        var data = Encoding.UTF8.GetBytes(input);
        var hash = XxHash3.HashToUInt64(data);
        return hash.ToString();
    }

    private async Task RunJobAsync(Dictionary<string, Task<List<ProductDTO>>> tasks)
    {
        _logger.LogInformation("Парсинг запущен");
        await Task.WhenAll(tasks.Select(x => x.Value));
        foreach (var task in tasks)
        {
            try
            {
                _pollingService.FinishJob(task.Key, task.Value.Result);
            }
            catch (Exception ex)
            {
                _pollingService.FailJob(task.Key, ex);
            }
        }
    }
}