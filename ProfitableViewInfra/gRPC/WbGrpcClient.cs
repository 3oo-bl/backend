using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Grpc.Net.Client;
using ProfitableViewApp.Interfaces;
using WbGrpc;

namespace ProfitableViewDataInfra.gRPC;

public class WbGrpcClient : IGrpcClient
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

    public IAsyncEnumerable<SearchResponse> SearchAsync(string itemName, int page)
    {
        Console.WriteLine("Парсинг вб начался");
        var request = new SearchRequest
        {
            ItemName = itemName,
            Page = page
        };
        var call = _client.Search(request);
        return call.ResponseStream.ReadAllAsync();
    }
}