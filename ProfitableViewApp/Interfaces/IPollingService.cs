using Microsoft.AspNetCore.Http;
using ProfitableViewApp.DTOS;

namespace ProfitableViewApp.Interfaces;

public interface IPollingService
{
    public bool AddJob(string token);
    public JobResult? GetJob(string token);
    public void FinishJob(string token, List<ProductDTO> result);
    public void FailJob(string token, Exception ex);
    public void AddRequest(string requestToken, List<string> jobTokens);
    public OrderProductsInfoItem? GetRequest(string token);
    public ParsingJobStates? GetRequestState(string requestToken);
    public List<ProductDTO>? GetOrderedProductList(string token, string? id, RequestResultsItem request);
}