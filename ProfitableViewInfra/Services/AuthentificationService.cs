using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;
using ProfitableViewDataInfra;

namespace ProfitableViewInfra.Services;

public class AuthentificationService
{
    private readonly DBContext _dbContext;
    private readonly ILogger _logger;
    private readonly PasswordHasher<string> _hasher;
    private readonly AuthTokenService _authTokenService;
    
    public AuthentificationService(DBContext dbContext, ILogger logger,
        PasswordHasher<string> hasher, AuthTokenService authTokenService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _hasher = hasher;
        _authTokenService = authTokenService;
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
        return Results.Ok(_authTokenService.GenerateAuthToken(user.Id.ToString()));
    }
}