using Microsoft.Extensions.Logging;
using ProfitableViewDataInfra.gRPC;
using ProfitableViewDataInfra.Parsers;
using ProfitableViewDataInfra.Searchers;
using ProfitableViewInfra.Searchers;

namespace ProfitableViewTests.ParsersTests;

[TestFixture]
public class OzonMarketplaceParserTests
{
    [Test]
    public void ParseResponse_Should_ReturnsDTOList()
    {
        var parser = new OzonMarketplaceParser(new Logger<OzonMarketplaceParser>(new LoggerFactory()),
            new HttpClient(), new OzonSearcher(new OzonGrpcClient()));
        var response = File.ReadAllText("ozonProducts.json");
        
        var parsedProducts = parser.ParseResponse(response);
        Assert.That(parsedProducts.Count, Is.GreaterThan(0));
    }
}