using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ProfitableViewDataInfra.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        this._config = config;
    }
    
    public string GenerateToken(string id)
    {
        var claims = new List<Claim> {new(ClaimTypes.PrimarySid, id)};

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
}