using System.Net;

namespace ProfitableViewDataInfra.Utils;

public class HttpClientFactory
{
    public HttpClient Create(CookieContainer cookies)
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All
        };
        
        return new HttpClient(handler);
    }
}