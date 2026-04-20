using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.gRPC;

namespace ProfitableViewDataInfra.Searchers;

public class WbSearcher : ISearcher
{
    private readonly WbGrpcClient _client;

    public WbSearcher(WbGrpcClient client)
    {
        _client = client;
    }
    
    public async Task<string> Search(string query, int page)
    {
        return await _client.SearchAsync(query, page);
    }
}