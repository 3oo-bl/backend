using Microsoft.Extensions.Logging;
using ProfitableViewDataInfra.gRPC;
using ProfitableViewDataInfra.Parsers;
using ProfitableViewDataInfra.Searchers;

namespace ProfitableViewTests.ParsersTests;

[TestFixture]
public class OzonMarketplaceParserTests
{
    [Test]
    public async Task A()
    {
        var parser = new OzonMarketplaceParser(new Logger<OzonMarketplaceParser>(new LoggerFactory()),
            new HttpClient(), new OzonSearcher(new OzonGrpcClient()));
        var quantity = 25;
        
        parser.ParseProductList("линейка", quantity);
        Assert.That(true, Is.False);
    }
}