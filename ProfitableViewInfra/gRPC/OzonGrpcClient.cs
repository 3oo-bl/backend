using System.Net.Security;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
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
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true
            }
        };

        var channel = GrpcChannel.ForAddress(
            "https://grpc_searcher:50051",
            new GrpcChannelOptions { HttpHandler = handler }
        );
        _client = new OzonParser.OzonParserClient(channel);
    }

    public IAsyncEnumerable<SearchResponse> SearchAsync(string itemName, int quantity)
    {
        Console.WriteLine("Парсинг озоза начался");
        var request = new OzonSearchRequest
        {
            ItemName = itemName,
            Quantity = quantity
        };
        IAsyncEnumerable<SearchResponse> response;
        try
        {
            var call = _client.Search(request);
            response = call.ResponseStream.ReadAllAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return response;
    }
}