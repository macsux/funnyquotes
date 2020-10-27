using System;
using System.ServiceModel;
using System.Threading.Tasks;
using FunnQuotesWcfClient;
using FunnyQuotesCommon;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery;
using IFunnyQuoteService = FunnyQuotesCommon.IFunnyQuoteService;

namespace FunnyQuotesUICore.Clients
{
    public class WcfFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly DiscoveryHttpClientHandlerBase _discoveryClient;
        private readonly IOptionsSnapshot<FunnyQuotesConfiguration> _config;
        private AsyncPolicyWrap _apiCallPolicy;

        public WcfFunnyQuotesClient(IDiscoveryClient discoveryClient, IOptionsSnapshot<FunnyQuotesConfiguration> config)
        {
            var circuitBreaker =  Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(15));
            var retryOnFailure = Policy.Handle<Exception>().RetryAsync(1);
            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(2));
            _apiCallPolicy = Policy.WrapAsync(circuitBreaker, timeout, retryOnFailure);
            
            
            _config = config;
            _discoveryClient = new DiscoveryHttpClientHandlerBase(discoveryClient);
        }

        public async Task<string> GetQuoteAsync()
        {
            return await Policy<string>
                .Handle<Exception>()
                .FallbackAsync(_config.Value.FailedMessage)
                .WrapAsync(_apiCallPolicy)
                .ExecuteAsync(async () =>
                {
                    var uri = await _discoveryClient.LookupServiceAsync(new Uri("http://FunnyQuotesLegacyService/FunnyQuoteserviceWCF.svc"));
                    var wcfClient = new FunnyQuoteServiceClient(FunnyQuoteServiceClient.EndpointConfiguration.BasicHttpBinding_IFunnyQuoteService, uri.ToString());
                    return await wcfClient.GetQuoteAsync();
                });
        }

        public string GetQuote()
        {
            throw new NotImplementedException();
        }
    }
}