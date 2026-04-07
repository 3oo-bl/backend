using ProfitableViewApp.Interfaces;
using ProfitableViewData.gRPC;

namespace ProfitableViewData.Searchers;

public class WbSearcher : ISearcher
{
    private readonly WbGrpcClient _client;

    public WbSearcher(WbGrpcClient client)
    {
        _client = client;
    }
    
    public Task<string> Search(string query)
    {
        return _client.SearchAsync(query, 10);
    }
}