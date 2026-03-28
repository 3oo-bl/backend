using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProfitableViewData.DTOS;

namespace ProfitableViewCore;

public static class EndpointsExtensions
{
    private static Dictionary<int, UserDTO> _fakeDB = new()
    {
        {
            0, new()
            {
                Name = "Антон", Email = "1", Password = "1",
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

    private static int i = 2;

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
            [Authorize] (int me, PrefsWeigthsDTO newPreferences) =>
        {
            return UpdatePreferencies(_fakeDB[me], newPreferences, logger);
        }).WithOpenApi();
        app.MapPost("/goods/search", [Authorize] ([FromBody] RequestStartDTO requestStartDto) =>
        {
            return StartParsing(requestStartDto);
        }).WithOpenApi();
        app.MapGet("/goods/search/{jobId}", [Authorize] (string jobId,
            [AsParameters] RequestResultsDTO requestResultsDto) =>
        {
            var result = GetProducts(jobId, requestResultsDto);
            if (result is null)
                return Results.NotFound();
            return Results.Ok(result);
        }).WithOpenApi();
        app.MapPost("/auth/register", (UserDTO newUser) =>
        {
            if (_fakeDB.Any(x => x.Value.Email == newUser.Email))
                return Results.Conflict();
            _fakeDB[i] = newUser;
            ++i;
            return Results.Ok();
        }).WithOpenApi();
        app.MapPost("/auth/login", (IConfiguration config, string email, string password) =>
        {
            if (!_fakeDB.Any(x => x.Value.Email == email && x.Value.Password == password))
                return Results.NotFound();
            var claims = new List<Claim> {new(ClaimTypes.Email, email)};
            var key = config["jwt:Key"];
            Console.WriteLine($"KEY: '{key}'");

            var token = new JwtSecurityToken(
                issuer: "",
                audience: "ProfitableViewAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["jwt:Key"]!)),
                    SecurityAlgorithms.HmacSha256));
            
            return Results.Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }).WithOpenApi();
    }

    private static HttpStatusCode UpdatePreferencies(UserDTO user, PrefsWeigthsDTO newPreferences, ILogger logger)
    {
        user.Preferences = newPreferences;
        logger.LogInformation("Обновлено.");
        return HttpStatusCode.NoContent;
    }

    private static List<ProductDTO>? GetProducts(string jobId, RequestResultsDTO requestResultsDto)
    {
        if (!_fakeFoundedGoods.ContainsKey(jobId))
            return null;
        return _fakeFoundedGoods[jobId].Skip(requestResultsDto.Skip).Take(requestResultsDto.Take).ToList();
    }

    private static string StartParsing(RequestStartDTO requestStartDto)
    {
        var found = _fakeGoods.Where(x => x.Name == requestStartDto.Item).ToList();
        _fakeFoundedGoods = new Dictionary<string, List<ProductDTO>> {{ "token", found}};
        return "token";
    }
}