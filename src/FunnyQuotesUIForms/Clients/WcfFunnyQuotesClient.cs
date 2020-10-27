using System;
using System.ServiceModel;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using Microsoft.Extensions.Options;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class WcfFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly IOptionsSnapshot<FunnyQuotesConfiguration> _config;
        private readonly EndpointClientHandler _dicoveryAddressResolver;

        public WcfFunnyQuotesClient(IDiscoveryClient discoveryClient, IOptionsSnapshot<FunnyQuotesConfiguration> config)
        {
            _config = config;
            _dicoveryAddressResolver = new EndpointClientHandler(discoveryClient);
        }

        public async Task<string> GetQuoteAsync()
        {
            var options = new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("Legacy"), HystrixCommandKeyDefault.AsKey("Legacy.Wcf"));
            var cmd = new HystrixCommand<string>(options,
                run: GetCookieRun,
                fallback: GetCookieFallback);
            return await cmd.ExecuteAsync();

        }

        public string GetCookieRun()
        {
            var channelFactory = new ChannelFactory<IFunnyQuoteService>("FunnyQuoteServiceWcf", _dicoveryAddressResolver.GetEndpointAddress("FunnyQuoteServiceWcf"));
            return channelFactory.CreateChannel().GetQuoteAsync().Result;
        }

        public string GetCookieFallback() => _config.Value.FailedMessage;
        

        public string GetQuote()
        {
            throw new NotImplementedException();
        }
    }
}