using System;
using System.ServiceModel;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class WcfFunnyQuotesClient : IFunnyQuoteservice
    {
        private readonly EndpointClientHandler _dicoveryAddressResolver;

        public WcfFunnyQuotesClient(IDiscoveryClient discoveryClient)
        {
            _dicoveryAddressResolver = new EndpointClientHandler(discoveryClient);
        }

        public async Task<string> GetCookieAsync()
        {
            var channelFactory = new ChannelFactory<IFunnyQuoteservice>("FunnyQuoteserviceWcf", _dicoveryAddressResolver.GetEndpointAddress("FunnyQuoteserviceWcf"));
            return await channelFactory.CreateChannel().GetCookieAsync();
        }

        public string GetCookie()
        {
            throw new NotImplementedException();
        }
    }
}