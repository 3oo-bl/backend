using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.gRPC;

namespace ProfitableViewInfra.Searchers;

public class OzonSearcher : ISearcher
{
    private readonly OzonGrpcClient _client;

    public OzonSearcher(OzonGrpcClient client)
    {
        _client = client;
    }

    public async Task<string> Search(string query, int targetValue)
    {
        return await _client.SearchAsync(query, targetValue);
    }
}