using Grpc.Net.Client;
using ProfitableViewApp.Interfaces;
using WbGrpc;

namespace ProfitableViewDataInfra.gRPC;

public class OzonGrpcClient : IGrpcClient
{
    private readonly OzonParser.OzonParserClient _client;
    public OzonGrpcClient()
    {
        var handler = new SocketsHttpHandler
        {
            UseProxy = false,
        };

        var channel = GrpcChannel.ForAddress(
            "https://127.0.0.1:50051",
            new GrpcChannelOptions { HttpHandler = handler }
        );
        _client = new OzonParser.OzonParserClient(channel);
    }

    public async Task<string> SearchAsync(string itemName, int quantity)
    {
        var request = new OzonSearchRequest
        {
            ItemName = itemName,
            Quantity = quantity
        };
        WbGrpc.SearchResponse response;
        try
        {
            response = _client.Search(request);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return response.RawJson;
    }
}