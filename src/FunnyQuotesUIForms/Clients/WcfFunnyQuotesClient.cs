using System;
using System.ServiceModel;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class WcfFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly IOptionsSnapshot<FunnyQuotesConfiguration> _config;
        private readonly ILogger<WcfFunnyQuotesClient> _logger;
        private readonly EndpointClientHandler _dicoveryAddressResolver;

        public WcfFunnyQuotesClient(IDiscoveryClient discoveryClient, IOptionsSnapshot<FunnyQuotesConfiguration> config, ILogger<WcfFunnyQuotesClient> logger)
        {
            _config = config;
            _logger = logger;
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
            try
            {
                var channelFactory = new ChannelFactory<IFunnyQuoteService>("FunnyQuoteServiceWcf", _dicoveryAddressResolver.GetEndpointAddress("FunnyQuoteServiceWcf"));
                return channelFactory.CreateChannel().GetQuoteAsync().Result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error connecting to backend service");
                throw;
            }
        }

        public string GetCookieFallback() => _config.Value.FailedMessage;
        

        public string GetQuote()
        {
            throw new NotImplementedException();
        }
    }
}