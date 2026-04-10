using Microsoft.Extensions.DependencyInjection;
using ProfitableViewApp.Interfaces;
using ProfitableViewDataInfra.Parsers;
using ProfitableViewDataInfra.Utils;
using ProfitableViewInfra.Services;

namespace ProfitableViewInfra;

public static class DIExtensions
{
    public static void BindParsers(this IServiceCollection services)
    {
        services.AddScoped<IMarketplaceParser, WbMarketplaceParser>();
    }

    public static void BindClientFactory(this IServiceCollection services)
    {
        services.AddScoped<HttpClientFactory>();
    }

    public static void BindInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<AuthentificationService>();
        services.AddScoped<UpdatePrefsService>();
    }
}