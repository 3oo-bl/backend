using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;
using WbGrpc;

namespace ProfitableViewData.gRPC;

public class WbGrpcClient
{
    private readonly WbParser.WbParserClient _client;

    public WbGrpcClient()
    {
        var handler = new SocketsHttpHandler
        {
            UseProxy = false,
        };

        var channel = GrpcChannel.ForAddress(
            "https://127.0.0.1:50051",
            new GrpcChannelOptions { HttpHandler = handler }
        );
        _client = new WbParser.WbParserClient(channel);
    }

    public async Task<string> SearchAsync(string itemName, int quantity)
    {
        var request = new SearchRequest
        {
            ItemName = itemName,
            Quantity = quantity
        };
        var response = await _client.SearchAsync(request);

        return response.RawJson;
    }
}