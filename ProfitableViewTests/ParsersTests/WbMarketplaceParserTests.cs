// using Microsoft.Extensions.Logging;
// using ProfitableViewDataInfra.gRPC;
// using ProfitableViewDataInfra.Parsers;
// using ProfitableViewDataInfra.Searchers;
//
// namespace ProfitableViewTests.ParsersTests;
//
// [TestFixture]
// public class WbMarketplaceParserTests
// {
//     [Test]
//     public async Task GetCatalog_Should_ReturnNormalizedJsonDocument()
//     {
//         var parser = new WbMarketplaceParser(new Logger<WbMarketplaceParser>(new  LoggerFactory()), new HttpClient(), new WbSearcher(new WbGrpcClient()));
//         var quantity = 105;
//         
//         var responce = await parser.ParseProductList("беспроводные наушники Sony WH-1000XM5", quantity);
//         
//         Assert.NotNull(responce);
//         Assert.That(responce.Count, Is.EqualTo(quantity));
//     }
//
//     [Test]
//     public async Task ParseResponse_Should_ReturnDTOList()
//     {
//         var parser = new WbMarketplaceParser(new Logger<WbMarketplaceParser>(new  LoggerFactory()), new HttpClient(), new WbSearcher(new WbGrpcClient()));
//         var response = File.ReadAllText("test_products.json");
//         
//         var parsedProducts = await parser.ParseResponse(response);
//         Assert.That(parsedProducts.Count, Is.GreaterThan(0));
//     }
// }