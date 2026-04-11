using Microsoft.AspNetCore.Http;
using ProfitableViewApp.DTOS;

namespace ProfitableViewApp.Interfaces;

public interface IPollingService
{
    public bool AddJob(string token);
    public ParsingJobStates? CheckJobStatus(string token);
    public void FinishJob(string token, List<ProductDTO> result);
    public void FailJob(string token, Exception ex);
    public JobResult GetJobResult(string token);
}