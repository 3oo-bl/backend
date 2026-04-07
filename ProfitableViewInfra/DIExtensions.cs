using Microsoft.Extensions.DependencyInjection;
using ProfitableViewApp.Interfaces;
using ProfitableViewData.Parsers;
using ProfitableViewData.Utils;

namespace ProfitableViewData;

public static class DIExtensions
{
    public static IServiceCollection BindParsers(this IServiceCollection services)
    {
        services.AddScoped<IMarketplaceParser, WbMarketplaceParser>();
        
        return services;
    }

    public static IServiceCollection BindClientFactory(this IServiceCollection services)
    {
        services.AddScoped<HttpClientFactory>();
        
        return services;
    }
}