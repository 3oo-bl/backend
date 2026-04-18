using WbGrpc;

namespace ProfitableViewApp.Interfaces;

public interface IGrpcClient
{
    Task<string> SearchAsync(string itemName, int page);
}