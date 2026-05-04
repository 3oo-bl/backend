using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfitableViewApp;
using ProfitableViewApp.DTOS;
using ProfitableViewApp.Interfaces;
using ProfitableViewApp.Services;
using ProfitableViewInfra.Services;

namespace ProfitableViewCore;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", (UserDTO newUser, AuthentificationService authService)
            => authService.Register(newUser)).WithOpenApi();
        app.MapPost("/auth/login", (AuthItem authItem, AuthentificationService authService)
            => authService.Login(authItem.Email, authItem.Password)).WithOpenApi();
        app.MapPatch("/users", [Authorize] (HttpContext context, UpdatePrefsService updatePrefsService,
            PrefsWeightsDTO newPreferences) =>
        {
            var id = context.User.FindFirst(ClaimTypes.PrimarySid)?.Value;
            return updatePrefsService.UpdatePrefs(int.Parse(id!), newPreferences);
        }).WithOpenApi();
        app.MapPost("/goods/search",
            [Authorize]([FromBody] RequestStartItem requestStartItem, ParseMarketService parseMarketService) =>
            parseMarketService.ParseProductList(requestStartItem));
        app.MapGet("/goods/search/{personalToken}", [Authorize] (HttpContext context, string personalToken,
            [AsParameters] RequestResultsItem requestResultsItem, IPollingService pollingService) =>
        {
            var status = pollingService.GetRequestState(personalToken);
            if (status is null)
                return Results.BadRequest();
            if (status is ParsingJobStates.Pending)
                return Results.Accepted();
            var id = context.User.FindFirst(ClaimTypes.PrimarySid)?.Value;
            var result = pollingService.GetOrderedProductList(personalToken, id, requestResultsItem);
            if (result is not null)
                return Results.Ok(result);
            return Results.Problem();
        }).WithOpenApi();
    }
}