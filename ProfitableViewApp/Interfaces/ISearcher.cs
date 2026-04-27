using Grpc.Core;
using WbGrpc;

namespace ProfitableViewApp.Interfaces;

public interface ISearcher
{
    IAsyncEnumerable<SearchResponse> Search(string query, int targetValue);
}