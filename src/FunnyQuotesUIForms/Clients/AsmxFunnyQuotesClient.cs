using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesUIForms.FunnyQuotesLegacyService;
using Microsoft.Extensions.Options;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class AsmxFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly IOptionsSnapshot<FunnyQuotesConfiguration> _config;
        private readonly DiscoveryHttpClientHandler _dicoveryAddressResolver;

        public AsmxFunnyQuotesClient(IDiscoveryClient discoveryClient, IOptionsSnapshot<FunnyQuotesConfiguration> config)
        {
            _config = config;
            _dicoveryAddressResolver = new DiscoveryHttpClientHandler(discoveryClient);
        }

        public string GetQuote()
        {
            var options = new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("Legacy"), HystrixCommandKeyDefault.AsKey("Cookie.Asmx"));
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

        public string GetCookieFallback() => _config.Value.FailedMessage;

        public Task<string> GetQuoteAsync()
        {
            return Task.Run(() => GetQuote());
        }
    }
}