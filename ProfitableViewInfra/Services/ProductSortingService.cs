using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;

namespace ProfitableViewInfra.Services;

public class ProductSortingService
{
    private readonly DBContext _dbContext;
    private readonly ILogger _logger;

    public ProductSortingService(DBContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public List<ProductDTO>? SortProductsByUserPreferences(List<ProductDTO> products, string? userId)
    {
        var user = _dbContext.Users.Include(userDto => userDto.Preferences)
            .FirstOrDefault(x => x.Id == int.Parse(userId!));
        if (user is null)
            return null;
        return SortProducts(products, user.Preferences);
    }

    internal List<ProductDTO>? SortProductsByUserPreferences(List<ProductDTO> products, PrefsWeightsDTO prefsWeights)
        => SortProducts(products, prefsWeights);

    private List<ProductDTO>? SortProducts(List<ProductDTO> products, PrefsWeightsDTO prefsWeights)
    {
        if (products.Count == 0) return products;
    
        var maxCost = products.Max(p => p.Cost);
        var maxRating = products.Max(p => p.Rating);
    
        var comparer = new ProductPrefsComparer(prefsWeights, maxCost, maxRating);
        return products.Order(comparer).ToList();
    }
}

class ProductPrefsComparer : IComparer<ProductDTO>
{
    private readonly PrefsWeightsDTO _prefsWeightsDto;
    private readonly float _maxCost;
    private readonly float _maxRating;

    public ProductPrefsComparer(PrefsWeightsDTO prefsWeightsDto, float maxCost, float maxRating)
    {
        _prefsWeightsDto = prefsWeightsDto;
        _maxCost = maxCost;
        _maxRating = maxRating;
    }

    public int Compare(ProductDTO? x, ProductDTO? y)
    {
        if (x is null) return y is null ? 0 : -1;
        if (y is null) return 1;
        var xNormCost = _maxCost > 0 ? 1 - (x.Cost / _maxCost) : 0;
        var yNormCost = _maxCost > 0 ? 1 - (y.Cost / _maxCost) : 0;

        var xNormRating = _maxRating > 0 ? x.Rating / _maxRating : 0;
        var yNormRating = _maxRating > 0 ? y.Rating / _maxRating : 0;

        var xAttractiveness = xNormCost * _prefsWeightsDto.Price + xNormRating * _prefsWeightsDto.Rating;
        var yAttractiveness = yNormCost * _prefsWeightsDto.Price + yNormRating * _prefsWeightsDto.Rating;

        return yAttractiveness.CompareTo(xAttractiveness);
    }
}