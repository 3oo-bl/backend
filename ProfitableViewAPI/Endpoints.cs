using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ProfitableViewApp.DTOS;
using ProfitableViewInfra.Services;

namespace ProfitableViewCore;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", (UserDTO newUser, AuthentificationService authService)
            => authService.Register(newUser)).WithOpenApi();
        app.MapPost("/auth/login", (string email, string password, AuthentificationService authService)
            => authService.Login(email, password)).WithOpenApi();
        app.MapPatch("/users", [Authorize] (HttpContext context, UpdatePrefsService updatePrefsService,
            PrefsWeigthsDTO newPreferences) =>
        {
            var id = context.User.FindFirst(ClaimTypes.PrimarySid)?.Value;
            return updatePrefsService.UpdatePrefs(int.Parse(id!), newPreferences);
        }).WithOpenApi();
    }
}