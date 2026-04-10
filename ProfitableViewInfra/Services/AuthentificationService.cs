using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewDataInfra;
using ProfitableViewDataInfra.Services;

namespace ProfitableViewInfra.Services;

public class AuthentificationService
{
    private readonly DBContext _dbContext;
    private readonly ILogger _logger;
    private readonly PasswordHasher<string> _hasher;
    private readonly TokenService _tokenService;
    
    public AuthentificationService(DBContext dbContext, ILogger logger,
        PasswordHasher<string> hasher, TokenService tokenService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public IResult Register(UserDTO newUser)
    {
        if (_dbContext.Users.Any(u => u.Email == newUser.Email))
            return Results.Conflict();
        var hashPassword = _hasher.HashPassword(newUser.Email, newUser.Password);
        newUser.Password = hashPassword;
        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();
        return Results.Ok();
    }

    public IResult Login(string email, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
        if (user is null)
            return Results.NotFound("Неверное имя пользователя или пароль");
        var result = _hasher.VerifyHashedPassword(email, user.Password, password);
        if (result is PasswordVerificationResult.Failed)
            return Results.NotFound("Неверное имя пользователя или пароль");
        return Results.Ok(_tokenService.GenerateToken(user.Id.ToString()));
    }
}