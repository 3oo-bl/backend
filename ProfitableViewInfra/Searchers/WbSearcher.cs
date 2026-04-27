using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.gRPC;
using WbGrpc;

namespace ProfitableViewDataInfra.Searchers;

public class WbSearcher : ISearcher
{
    private readonly WbGrpcClient _client;

    public WbSearcher(WbGrpcClient client)
    {
        _client = client;
    }
    
    public IAsyncEnumerable<SearchResponse> Search(string query, int page)
    {
        return _client.SearchAsync(query, page);
    }
}