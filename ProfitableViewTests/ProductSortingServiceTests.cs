using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewDataInfra.gRPC;
using ProfitableViewDataInfra.Parsers;
using ProfitableViewDataInfra.Searchers;
using ProfitableViewInfra;
using ProfitableViewInfra.Services;

namespace ProfitableViewTests;

[TestFixture]
public class ProductSortingServiceTests
{
    private ProductSortingService _productSortingService;
    
    [SetUp]
    public void Setup()
    {
        _productSortingService = new ProductSortingService(null!, null!);
    }
    
    [Test]
    public void ProductSortingService_CheaperFirst()
    {
        var prefs = new PrefsWeightsDTO { Price = 0.8f, Rating = 0.2f };
        var products = new List<ProductDTO>
        {
            new() { Id = "1", Cost = 1000, Rating = 5 },
            new() { Id = "2", Cost = 100,  Rating = 1 },
            new() { Id = "3", Cost = 500,  Rating = 3 },
        };

        var service = new ProductSortingService(null, null);
        var result = service.SortProductsByUserPreferences(products, prefs);

        Assert.That(result![0].Id, Is.EqualTo("2"));
        Assert.That(result![1].Id, Is.EqualTo("3"));
        Assert.That(result![2].Id, Is.EqualTo("1"));
    }
    
    [Test]
    public void ProductSortingService_HigherRatingFirst()
    {
        var prefs = new PrefsWeightsDTO { Price = 0.2f, Rating = 0.8f };
        var products = new List<ProductDTO>
        {
            new() { Id = "1", Cost = 1000, Rating = 5 },
            new() { Id = "2", Cost = 100,  Rating = 1 },
            new() { Id = "3", Cost = 500,  Rating = 3 },
        };

        var service = new ProductSortingService(null, null);
        var result = service.SortProductsByUserPreferences(products, prefs);

        Assert.That(result![0].Id, Is.EqualTo("1"));
        Assert.That(result![1].Id, Is.EqualTo("3"));
        Assert.That(result![2].Id, Is.EqualTo("2"));
    }
    
    [Test]
    public void ProductSortingService_EqualPreferencies()
    {
        var prefs = new PrefsWeightsDTO { Price = 0.5f, Rating = 0.5f };
        var products = new List<ProductDTO>
        {
            new() { Id = "1", Cost = 1000, Rating = 5 },
            new() { Id = "2", Cost = 750,  Rating = 2 },
            new() { Id = "3", Cost = 500,  Rating = 3 },
        };

        var service = new ProductSortingService(null, null);
        var result = service.SortProductsByUserPreferences(products, prefs);

        Assert.That(result![0].Id, Is.EqualTo("3"));
        Assert.That(result![1].Id, Is.EqualTo("1"));
        Assert.That(result![2].Id, Is.EqualTo("2"));
    }
}