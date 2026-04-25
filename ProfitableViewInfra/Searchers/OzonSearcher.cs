using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.gRPC;
using WbGrpc;

namespace ProfitableViewInfra.Searchers;

public class OzonSearcher : ISearcher
{
    private readonly OzonGrpcClient _client;

    public OzonSearcher(OzonGrpcClient client)
    {
        _client = client;
    }

    public IAsyncEnumerable<SearchResponse> Search(string query, int targetValue)
    {
        return _client.SearchAsync(query, targetValue);
    }
}