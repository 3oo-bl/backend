using Microsoft.Extensions.Logging;
using ProfitableViewApp;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;

namespace ProfitableViewInfra.Services;

public class InMemoryPollingService : IPollingService
{
    private readonly Dictionary<string, JobResult> _jobs = new();
    private readonly ILogger<InMemoryPollingService> _logger;

    public InMemoryPollingService(ILogger<InMemoryPollingService> logger)
    {
        _logger = logger;
    }
    
    public bool AddJob(string token)
    {
        if (_jobs.ContainsKey(token))
        {
            _logger.LogError("Джоба с айди {token} уже существует!", token);
            return false;
        }
        _logger.LogTrace("Джоба {token} добавлена!", token);
        Console.WriteLine($"Джоба {token} добавлена!");
        _jobs.Add(token, new JobResult{Token = token, State = ParsingJobStates.Pending});
        return true;
    }

    public ParsingJobStates? CheckJobStatus(string token)
    {
        if (!_jobs.ContainsKey(token))
        {
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её проверить!");
            return null;
        }
        return _jobs[token].State;
    }

    public void FinishJob(string token, List<ProductDTO> result)
    {
        if (!_jobs.ContainsKey(token))
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её обновить!");
        else
        {
            _jobs[token].State = ParsingJobStates.Completed;
            _jobs[token].Products = result;
        }
    }

    public void FailJob(string token, Exception ex)
    {
        if (!_jobs.ContainsKey(token))
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её обновить!");
        else
        {
            _logger.LogError($"Исключение! {ex.Message}");
            _jobs[token].State = ParsingJobStates.Failed;
            _jobs[token].Exception = ex;
        }
    }

    public List<ProductDTO>? GetProductList(string token, RequestResultsDTO request)
    {
        var jobResult = _jobs[token];
        if (jobResult.State is ParsingJobStates.Failed)
            return null; // #TODO Exception не дремлет, он лежит в jobResult :(
        var products = jobResult.Products!.Skip(request.Skip)
            .Take(request.Take);
        if (request.MinPrice is not null)
            products = products.Where(x => x.Cost > request.MinPrice.Value);
        if (request.MaxPrice is not null)
            products = products.Where(x => x.Cost < request.MaxPrice.Value);
        if (request.OrderBy is not null)
        {
            if (request.OrderBy == "asc")
                products = products.OrderBy(x => x.Cost);
            if (request.OrderBy == "desc")
                products = products.OrderByDescending(x => x.Cost);
        }
        return products.ToList();
    }
}