using System.Net;

namespace ProfitableViewTests;

internal interface ISessionProvider
{
    Task<CookieContainer> GetCookieContainerAsync();
}