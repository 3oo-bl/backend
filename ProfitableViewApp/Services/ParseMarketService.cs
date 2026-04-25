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

    public string? ParseProductList(RequestStartDTO request)
    {
        var token = GetHash(request.Item);
        if (!_pollingService.AddJob(token))
            return token;
        var tasks = new List<Task<List<ProductDTO>>>();
        if (request.Markets is null)
        {
            _logger.LogInformation("Магазинов в запросе не было, парсим всё.");
            foreach (var parser in _parsers.Values)
                tasks.Add(parser.ParseProductList(parser.MarketName, request.Quantity));
        }
        foreach (var market in request.Markets)
        {
            if (_parsers.TryGetValue(market, out IMarketplaceParser parser))
                tasks.Add(parser.ParseProductList(request.Item, request.Quantity));
            else
                _logger.LogWarning($"Парсера для магазина {market} не существует");
        }

        if (tasks.Count == 0)
        {
            _logger.LogWarning("Нет подходящих под запрос парсеров или список магазинов пуст.");
            return null;
        }
        _ = Task.Run(() => RunJobAsync(token, tasks));
        return token;
    }

    private string GetHash(string input)
    {
        var data = Encoding.UTF8.GetBytes(input);
        var hash = XxHash3.HashToUInt64(data);
        return hash.ToString();
    }

    private async Task RunJobAsync(string token, List<Task<List<ProductDTO>>> tasks)
    {
        try
        {
            _logger.LogInformation("Парсинг запущен");
            var result = await Task.WhenAll(tasks);
            _pollingService.FinishJob(token, result.SelectMany(x => x).ToList());
        }
        catch (Exception ex)
        {
            _pollingService.FailJob(token, ex);
        }
    }
}