using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesUIForms.Web_References.FunnyQuotesLegacyService;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class AsmxFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly DiscoveryHttpClientHandler _dicoveryAddressResolver;

        public AsmxFunnyQuotesClient(IDiscoveryClient discoveryClient)
        {
            _dicoveryAddressResolver = new DiscoveryHttpClientHandler(discoveryClient);
        }

        public string GetCookie()
        {
            var options = new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("Cookie"), HystrixCommandKeyDefault.AsKey("Cookie.Asmx"));
            var cmd = new HystrixCommand<string>(options,
                run: GetCookieRun,
                fallback: GetCookieFallback);
            return cmd.Execute();
        }

        private string GetCookieRun()
        {
            var client = new FunnyQuoteserviceLegacy();
            var uri = "http://FunnyQuotesLegacyService" + new Uri(client.Url).AbsolutePath;
            client.Url = _dicoveryAddressResolver.LookupService(new Uri(uri)).ToString();
            return client.GetCookie();
        }

        private string GetCookieFallback()
        {
            return "Failure is not an option -- it comes bundled with Windows.";
        }

        public Task<string> GetCookieAsync()
        {
            return Task.Run(() => GetCookie());
        }
    }
}