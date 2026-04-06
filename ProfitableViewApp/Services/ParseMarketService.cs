using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;

namespace ProfitableViewApp.Services;

public class ParseMarketService
{
    private readonly Dictionary<string, IParser> _parsers;
    private readonly ILogger _logger;
    
    public ParseMarketService(IEnumerable<IParser> parsers, ILogger logger)
    {
        _parsers = new Dictionary<string, IParser>();
        foreach (var parser in parsers)
        {
            if (_parsers.ContainsKey(parser.MarketName))
            {
                throw new InvalidOperationException($"Сервис {parser.MarketName} пытался добавиться дважды!");
            }

            _parsers.Add(parser.MarketName, parser);
        }
        this._logger = logger;
    }

    public async Task<List<ProductDTO>> ParseProductList(RequestStartDTO request)
    {
        var tasks = new List<Task<List<ProductDTO>>>();
        foreach (var market in request.Markets)
        {
            if (_parsers.TryGetValue(market, out IParser parser))
            {
                tasks.Add(parser.ParseProductList(market, request.RetryCount, request.RetryDelay));
            }
            else
                _logger.LogWarning($"Парсера для магазина {market} не существует");
        }

        if (tasks.Count == 0)
        {
            _logger.LogWarning("Нет подходящих под запрос парсеров.");
            return new List<ProductDTO>();
        }
        var result = await Task.WhenAll(tasks);
        return result.SelectMany(x => x).ToList();
    }
}