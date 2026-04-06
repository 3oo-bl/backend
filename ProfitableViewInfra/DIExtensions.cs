using Microsoft.Extensions.DependencyInjection;
using ProfitableViewApp.Interfaces;
using ProfitableViewData.Parsers;

namespace ProfitableViewData;

public static class DIExtensions
{
    public static IServiceCollection BindParsers(this IServiceCollection services)
    {
        services.AddScoped<IParser, WBParser>();
        
        return services;
    }
}