using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProfitableViewApp.DTOS;

namespace ProfitableViewInfra.Services;

public class UpdatePrefsService
{
    private readonly DBContext _dbContext;
    private readonly ILogger _logger;
    
    public UpdatePrefsService(DBContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public IResult UpdatePrefs(int userId, PrefsWeigthsDTO newPreferences)
    {
        var user = _dbContext.Users.FirstOrDefault(x => x.Id == userId);
        if (user == default)
            return Results.BadRequest("Что-то не так с ID пользователя");
        user.Preferences = newPreferences;
        _dbContext.SaveChanges();
        return Results.NoContent();
    }
}