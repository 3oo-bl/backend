using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProfitableViewApp;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;

namespace ProfitableViewInfra;

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
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её проверить!");
        else
        {
            _jobs[token].State = ParsingJobStates.Completed;
            _jobs[token].Products = result;
        }
    }

    public void FailJob(string token, Exception ex)
    {
        if (!_jobs.ContainsKey(token))
            _logger.LogWarning($"Джоба с айди {token} не найдена, но вы пытаетесь её проверить!");
        else
        {
            _logger.LogError($"Исключение! {ex.Message}");
            _jobs[token].State = ParsingJobStates.Failed;
            _jobs[token].Exception = ex;
        }
    }

    public JobResult GetJobResult(string token)
    {
        var job = _jobs[token];
        _jobs.Remove(token);
        return job;
    }
}