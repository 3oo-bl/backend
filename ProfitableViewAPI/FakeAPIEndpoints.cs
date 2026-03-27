using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProfitableViewData.DTOS;

namespace ProfitableViewCore;

public static class EndpointsExtensions
{
    private static Dictionary<int, UserDTO> _fakeDB = new()
    {
        {
            0, new()
            {
                Name = "Антон", Email = "email@email.com", Password = "aaaaa",
                Preferences = new() { Price = 0.5f, Delivery = 0.1f, SellerRating = 0.4f }
            }
        },
        {
            1, new()
            {
                Name = "Геннадий", Email = "oleg@pochta.com", Password = "11111",
                Preferences = new() { Price = 0.5f, Delivery = 0.1f, SellerRating = 0.4f }
            }
        },
    };

    private static List<ProductDTO> _fakeGoods = new()
    {
        new()
        {
            Id = "1234", Name = "1", Cost = 1895, CostWithDiscount = 1819,
            Category = "Бытовая техника", Cashback = null, Brand = "AMI", Seller = "AMI", SellerRating = 4.9f,
            Rating = 4.5f, Reviews = 1056, Remaining = 40
        },
        new()
        {
            Id = "5678", Name = "2", Cost = 999999, CostWithDiscount = 1,
            Category = "Бытовая техника", Cashback = null, Brand = "AMI", Seller = "Not a scammer",
            SellerRating = 1.3f,
            Rating = 1.1f, Reviews = 1200, Remaining = 40
        },
    };

    private static Dictionary<string, List<ProductDTO>> _fakeFoundedGoods = new();

    public static void MapFakeEndpoints(this WebApplication app, ILogger logger)
    {
        app.MapPatch("/users/{me}",
        (int me, PrefsWeigthsDTO newPreferences) =>
        {
            return UpdatePreferencies(_fakeDB[me], newPreferences, logger);
        }).WithOpenApi();
        app.MapPost("/goods/search", ([FromBody] RequestDTO requestDto) =>
        {
            return StartParsing(requestDto);
        }).WithOpenApi();
        app.MapGet("/goods/search/{jobId}", ([FromQuery] string jobId,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromQuery] int? minPrice,
            [FromQuery] int? maxPrice,
            [FromQuery] string? sortBy,
            [FromQuery] string? orderBy) =>
        {
            return GetProducts(jobId, skip, take,
                minPrice, maxPrice, sortBy, orderBy);
        }).WithOpenApi();
    }

    private static HttpStatusCode UpdatePreferencies(UserDTO user, PrefsWeigthsDTO newPreferences, ILogger logger)
    {
        user.Preferences = newPreferences;
        logger.LogInformation("Обновлено.");
        return HttpStatusCode.NoContent;
    }

    private static List<ProductDTO> GetProducts(string jobId, int skip, int take,
        int? minPrice, int? maxPrice, string? sortBy, string? orderBy)
    {
        if (!_fakeFoundedGoods.ContainsKey(jobId))
            return null;
        return _fakeFoundedGoods[jobId].Skip(skip).Take(take).ToList();
    }

    private static string StartParsing(RequestDTO requestDto)
    {
        var found = _fakeGoods.Where(x => x.Name == requestDto.Item).ToList();
        _fakeFoundedGoods = new Dictionary<string, List<ProductDTO>> {{ "token", found}};
        return "token";
    }
}