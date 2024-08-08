using System;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesUIForms.FunnyQuotesLegacyService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class AsmxFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly ILogger _logger;
        private readonly DiscoveryHttpClientHandlerBase _dicoveryAddressResolver;

        public AsmxFunnyQuotesClient(IDiscoveryClient discoveryClient, ILogger<AsmxFunnyQuotesClient> logger)
        {
            _logger = logger;
            _dicoveryAddressResolver = new DiscoveryHttpClientHandlerBase(discoveryClient);
        }

        public string GetQuote()
        {
            var client = new FunnyQuoteserviceLegacy();
            var uri = "http://FunnyQuotesLegacyService" + new Uri(client.Url).AbsolutePath;
            client.Url = _dicoveryAddressResolver.LookupService(new Uri(uri)).ToString();
            _logger.LogInformation("Calling ASMX method {Method} on {Uri}", nameof(client.GetCookie), client.Url);
            return client.GetCookie();
        }

        public Task<string> GetQuoteAsync()
        {
            return Task.Run(GetQuote);
        }
    }
}