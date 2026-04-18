using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.gRPC;

namespace ProfitableViewDataInfra.Searchers;

public class OzonSearcher : ISearcher
{
    private readonly OzonGrpcClient _client;

    public OzonSearcher(OzonGrpcClient client)
    {
        _client = client;
    }

    public Task<string> Search(string query, int targetValue)
    {
        return _client.SearchAsync(query, targetValue);
    }
}