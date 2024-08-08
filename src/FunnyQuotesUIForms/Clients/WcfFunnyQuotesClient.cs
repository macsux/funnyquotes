using System;
using System.ServiceModel;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class WcfFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly ILogger<WcfFunnyQuotesClient> _logger;
        private readonly EndpointClientHandler _dicoveryAddressResolver;

        public WcfFunnyQuotesClient(IDiscoveryClient discoveryClient, ILogger<WcfFunnyQuotesClient> logger)
        {
            _logger = logger;
            _dicoveryAddressResolver = new EndpointClientHandler(discoveryClient);
        }

        public async Task<string> GetQuoteAsync()
        {
            try
            {
                var endpointAddress = _dicoveryAddressResolver.GetEndpointAddress("FunnyQuoteServiceWcf");
                _logger.LogInformation("Calling WCF method {Method} on {Uri}", nameof(IFunnyQuoteService.GetQuoteAsync), endpointAddress);
                var channelFactory = new ChannelFactory<IFunnyQuoteService>("FunnyQuoteServiceWcf", endpointAddress);
                return await channelFactory.CreateChannel().GetQuoteAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error connecting to backend service");
                throw;
            }
            
        }

        public string GetQuote()
        {
            throw new NotImplementedException();
        }
    }
}