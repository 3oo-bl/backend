using Microsoft.Extensions.Logging;
using ProfitableViewApp.Interfaces;
using ProfitableViewData.Parsers;

namespace ProfitableViewTests.ParsersTests;

[TestFixture]
public class WBParserTests
{
    [Test]
    public async Task GetCatalog_Should_ReturnNormalizedJsonDocument()
    {
        var parser = new WBParser(new Logger<WBParser>(new LoggerFactory()), new HttpClient());
        
        var responce = await parser.GetCatalog();
        
        Assert.NotNull(responce);
    }
}