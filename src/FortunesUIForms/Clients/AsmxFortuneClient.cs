using System;
using System.Threading.Tasks;
using FortunesCommon;
using FortunesUIForms.Web_References.FortunesLegacyService;
using Steeltoe.Common.Discovery;

namespace FortunesUIForms.Clients
{
    public class AsmxFortuneClient : IFortuneService
    {
        private readonly DiscoveryHttpClientHandler _dicoveryAddressResolver;

        public AsmxFortuneClient(IDiscoveryClient discoveryClient)
        {
            _dicoveryAddressResolver = new DiscoveryHttpClientHandler(discoveryClient);
        }

        public string GetCookie()
        {
            var client = new FortuneServiceLegacy();
            var uri = "http://FortunesLegacyService" + new Uri(client.Url).AbsolutePath;
            client.Url = _dicoveryAddressResolver.LookupService(new Uri(uri)).ToString();
            return client.GetCookie();
        }

        public Task<string> GetCookieAsync()
        {
            return Task.Run(() => GetCookie());
        }
    }
}