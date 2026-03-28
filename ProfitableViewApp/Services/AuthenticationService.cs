using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ProfitableViewApp.Services;

public class AuthenticationService
{
    private readonly IConfiguration _config;

    public AuthenticationService(IConfiguration config)
    {
        this._config = config;
    }
    
    public string GenerateToken(string email)
    {
        var claims = new List<Claim> {new(ClaimTypes.Email, email)};

        var token = new JwtSecurityToken(
            issuer: "",
            audience: "ProfitableViewAPI",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:Key"]!)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public IResult Login(string email, string password)
    {
        throw new NotImplementedException(); // #TODO логика логина для реального API
        // if (!_fakeDB.Any(x => x.Value.Email == email && x.Value.Password == password))
        //     return Results.NotFound();
        // return Results.Ok(GenerateToken(email));
    }
}