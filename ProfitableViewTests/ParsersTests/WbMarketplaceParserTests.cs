using Microsoft.Extensions.Logging;
using ProfitableViewData.gRPC;
using ProfitableViewData.Parsers;
using ProfitableViewData.Searchers;

namespace ProfitableViewTests.ParsersTests;

[TestFixture]
public class WbMarketplaceParserTests
{
    [Test]
    public async Task GetCatalog_Should_ReturnNormalizedJsonDocument()
    {
        var parser = new WbMarketplaceParser(new Logger<WbMarketplaceParser>(new  LoggerFactory()), new HttpClient(), new WbSearcher(new WbGrpcClient()));
        
        var responce = await parser.ParseProductList("вентилятор напольный", 5);
        
        Assert.NotNull(responce);
    }
}