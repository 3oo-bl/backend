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

    private string JobKey(string token) => $"job:{token}";
    private string RequestKey(string token) => $"request:{token}";
    
    public bool AddJob(string token)
    {
        var job = new JobResult
        {
            Token = token,
            State = ParsingJobStates.Pending
        };
        var json = JsonSerializer.Serialize(job);

        return _db.StringSet(
            JobKey(token),
            json,
            TimeSpan.FromMinutes(30),
            When.NotExists
        );
    }

    public JobResult? GetJob(string token)
    {
        var value = _db.StringGet(JobKey(token));
        if (value.IsNull) return null;

        return JsonSerializer.Deserialize<JobResult>(value!);
    }

    public void FinishJob(string token, List<ProductDTO> results)
    {
        if (!_db.KeyExists(JobKey(token)))
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
            _db.StringSet(JobKey(token), JsonSerializer.Serialize(result), TimeSpan.FromMinutes(30));
        }
    }

    public void FailJob(string token, Exception ex)
    {
        if (!_db.KeyExists(JobKey(token)))
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
            _db.StringSet(JobKey(token), JsonSerializer.Serialize(result), TimeSpan.FromMinutes(1));
        }
    }

    public void AddRequest(string requestToken, List<string> jobTokens)
    {
        var tokens = new OrderProductsInfoItem(jobTokens);

        _db.StringSet(
            RequestKey(requestToken),
            JsonSerializer.Serialize(tokens),
            TimeSpan.FromMinutes(30));
    }

    public OrderProductsInfoItem? GetRequest(string token)
    {
        var value = _db.StringGet(RequestKey(token));
        if (value.IsNull)
        {
            _logger.LogWarning($"Запрос с айди {token} не найден, но вы пытаетесь его проверить!");
            return null;
        }

        return JsonSerializer.Deserialize<OrderProductsInfoItem>(value!);
    }

    public ParsingJobStates? GetRequestState(string requestToken)
    {
        var request = GetRequest(requestToken);
        if (request is null)
            return null;
        
        var states = new List<ParsingJobStates>();
        foreach (var jobToken in request.Tokens)
        {
            var job = GetJob(jobToken);
            if (job is null)
                continue;
            states.Add(job.State);
        }
        if (states.Count == 0)
            return null;
        
        if (states.Any(x => x is ParsingJobStates.Failed))
            return ParsingJobStates.Failed;
        if (states.Any(x => x is ParsingJobStates.Pending))
            return ParsingJobStates.Pending;
        
        return ParsingJobStates.Completed;
    }

    public List<ProductDTO>? GetOrderedProductList(string token, string? id, RequestResultsItem request)
    {
        var requestResult = GetRequest(token);
        if (requestResult is null)
            return null;
        var products = new List<ProductDTO>();
        foreach (var jobToken in requestResult.Tokens)
        {
            var job = GetJob(jobToken);
            if (job is null || job.State is ParsingJobStates.Failed)
                return null; // #TODO Exception не дремлет, он лежит в jobResult :(
            products.AddRange(job.Products!);
        }
        var sortedProducts = _productSortingService
            .SortProductsByUserPreferences(products, id)
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