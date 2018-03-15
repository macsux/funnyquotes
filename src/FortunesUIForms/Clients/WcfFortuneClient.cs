using System;
using System.ServiceModel;
using System.Threading.Tasks;
using FortunesCommon;
using Steeltoe.Common.Discovery;

namespace FortunesUIForms.Clients
{
    public class WcfFortuneClient : IFortuneService
    {
        private readonly EndpointClientHandler _dicoveryAddressResolver;

        public WcfFortuneClient(IDiscoveryClient discoveryClient)
        {
            _dicoveryAddressResolver = new EndpointClientHandler(discoveryClient);
        }

        public async Task<string> GetCookieAsync()
        {
            var channelFactory = new ChannelFactory<IFortuneService>("FortuneServiceWcf", _dicoveryAddressResolver.GetEndpointAddress("FortuneServiceWcf"));
            return await channelFactory.CreateChannel().GetCookieAsync();
        }

        public string GetCookie()
        {
            throw new NotImplementedException();
        }
    }
}