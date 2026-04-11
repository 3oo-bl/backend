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
        app.MapPost("/auth/login", (string email, string password, AuthentificationService authService)
            => authService.Login(email, password)).WithOpenApi();
        app.MapPatch("/users", [Authorize] (HttpContext context, UpdatePrefsService updatePrefsService,
            PrefsWeigthsDTO newPreferences) =>
        {
            var id = context.User.FindFirst(ClaimTypes.PrimarySid)?.Value;
            return updatePrefsService.UpdatePrefs(int.Parse(id!), newPreferences);
        }).WithOpenApi();
        app.MapPost("/goods/search",
            [Authorize]([FromBody] RequestStartDTO requestStartDto, ParseMarketService parseMarketService) =>
            parseMarketService.ParseProductList(requestStartDto));
        app.MapGet("/goods/search/{jobId}", [Authorize] (string jobId,
            [AsParameters] RequestResultsDTO requestResultsDto, IPollingService pollingService) =>
        {
            var status = pollingService.CheckJobStatus(jobId);
            if (status is null)
                return Results.BadRequest();
            if (status is ParsingJobStates.Pending)
                return Results.Accepted();
            var result = pollingService.GetJobResult(jobId);
            if (result.Products is not null)
                return Results.Ok(result.Products);
            return Results.Problem();
        }).WithOpenApi();
    }
}