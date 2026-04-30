using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProfitableViewApp;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;
using StackExchange.Redis;

namespace ProfitableViewInfra.Services;

public class RedisPollingService : IPollingService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisPollingService> _logger;
    private readonly ProductSortingService _productSortingService;

    public RedisPollingService(ILogger<RedisPollingService> logger, IConnectionMultiplexer redis,
        ProductSortingService productSortingService)
    {
        _logger = logger;
        _db = redis.GetDatabase();
        _productSortingService = productSortingService;
    }
    
    public bool AddJob(string token)
    {
        var job = new JobResult
        {
            Token = token,
            State = ParsingJobStates.Pending
        };
        var json = JsonSerializer.Serialize(job);

        return _db.StringSet(
            token,
            json,
            TimeSpan.FromMinutes(30),
            When.NotExists
        );
    }

    public ParsingJobStates? CheckJobStatus(string token)
    {
        if (!_db.KeyExists(token))
        {
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её проверить!");
            return null;
        }

        var state = JsonSerializer.Deserialize<JobResult>(_db.StringGet(token)!)!.State;
        _logger.LogInformation(state.ToString());
        return state;
    }

    public void FinishJob(string token, List<ProductDTO> results)
    {
        if (!_db.KeyExists(token))
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её обновить!");
        else
        {
            _logger.LogInformation($"Джоба {token} завершена!");
            var result = new JobResult
            {
                Token = token,
                State = ParsingJobStates.Completed,
                Products = results
            };
            _db.StringSet(token, JsonSerializer.Serialize(result), TimeSpan.FromMinutes(30));
        }
    }

    public void FailJob(string token, Exception ex)
    {
        if (!_db.KeyExists(token))
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её обновить!");
        else
        {
            _logger.LogInformation($"Джоба {token} завершена с ошибкой {ex.Message}!");
            var result = new JobResult
            {
                Token = token,
                State = ParsingJobStates.Failed,
                Exception = ex
            };
            _db.StringSet(token, JsonSerializer.Serialize(result), TimeSpan.FromMinutes(1));
        }
    }

    public List<ProductDTO>? GetOrderedProductList(string token, string? id, RequestResultsDTO request)
    {
        var jobResult = JsonSerializer.Deserialize<JobResult>(_db.StringGet(token)!)!;
        if (jobResult.State is ParsingJobStates.Failed)
            return null; // #TODO Exception не дремлет, он лежит в jobResult :(
        var sortedProducts = _productSortingService
            .SortProductsByUserPreferences(jobResult.Products!, id)
            ?.Skip(request.Skip)
            .Take(request.Take);
        if (sortedProducts is null)
            return null;
        if (request.MinPrice is not null)
            sortedProducts = sortedProducts.Where(x => x.Cost > request.MinPrice.Value);
        if (request.MaxPrice is not null)
            sortedProducts = sortedProducts.Where(x => x.Cost < request.MaxPrice.Value);
        if (request.OrderBy is not null)
        {
            if (request.OrderBy == "asc")
                sortedProducts = sortedProducts.OrderBy(x => x.Cost);
            if (request.OrderBy == "desc")
                sortedProducts = sortedProducts.OrderByDescending(x => x.Cost);
        }
        return sortedProducts.ToList();
    }
}