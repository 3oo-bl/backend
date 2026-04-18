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
        foreach (var parser in parsers)
        {
            if (_parsers.ContainsKey(parser.MarketName))
            {
                throw new InvalidOperationException($"Сервис {parser.MarketName} пытался добавиться дважды!");
            }

            _parsers.Add(parser.MarketName, parser);
        }
        _logger = logger;
        _pollingService = pollingService;
    }

    public string? ParseProductList(RequestStartDTO request)
    {
        var tasks = new List<Task<List<ProductDTO>>>();
        if (request.Markets is null)
        {
            _logger.LogInformation("Магзинов в запросе не было, парсим всё.");
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
        var token = Guid.NewGuid().ToString("N");
        _pollingService.AddJob(token);
        _ = Task.Run(() => RunJobAsync(token, tasks));
        return token;
    }

    private async Task RunJobAsync(string token, List<Task<List<ProductDTO>>> tasks)
    {
        try
        {
            var result = await Task.WhenAll(tasks);
            _pollingService.FinishJob(token, result.SelectMany(x => x).ToList());
        }
        catch (Exception ex)
        {
            _pollingService.FailJob(token, ex);
        }
    }
}