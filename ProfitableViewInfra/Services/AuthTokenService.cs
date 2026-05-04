using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ProfitableViewInfra.Services;

public class AuthTokenService
{
    private readonly IConfiguration _config;

    public AuthTokenService(IConfiguration config)
    {
        this._config = config;
    }
    
    public string GenerateAuthToken(string id)
    {
        var claims = new List<Claim> {new(ClaimTypes.PrimarySid, id)};

        var token = new JwtSecurityToken(
            issuer: "",
            audience: "ProfitableViewAPI",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:Key"]!)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}