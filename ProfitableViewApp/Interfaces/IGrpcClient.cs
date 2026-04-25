using WbGrpc;

namespace ProfitableViewApp.Interfaces;

public interface IGrpcClient
{
    IAsyncEnumerable<SearchResponse> SearchAsync(string itemName, int page);
}