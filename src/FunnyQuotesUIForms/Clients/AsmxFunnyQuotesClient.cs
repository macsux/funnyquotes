using System;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesUIForms.Web_References.FunnyQuotesLegacyService;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class AsmxFunnyQuotesClient : IFunnyQuoteservice
    {
        private readonly DiscoveryHttpClientHandler _dicoveryAddressResolver;

        public AsmxFunnyQuotesClient(IDiscoveryClient discoveryClient)
        {
            _dicoveryAddressResolver = new DiscoveryHttpClientHandler(discoveryClient);
        }

        public string GetCookie()
        {
            var client = new FunnyQuoteserviceLegacy();
            var uri = "http://FunnyQuotesLegacyService" + new Uri(client.Url).AbsolutePath;
            client.Url = _dicoveryAddressResolver.LookupService(new Uri(uri)).ToString();
            return client.GetCookie();
        }

        public Task<string> GetCookieAsync()
        {
            return Task.Run(() => GetCookie());
        }
    }
}