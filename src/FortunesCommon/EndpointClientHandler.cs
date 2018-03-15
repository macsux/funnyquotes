using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using Steeltoe.Common.Discovery;

namespace FortunesCommon
{
    public class EndpointClientHandler : DiscoveryHttpClientHandler
    {
        public EndpointClientHandler(IDiscoveryClient client) : base(client)
        {
        }

        public EndpointAddress GetEndpointAddress(string endpointName)
        {
            var clientSection = (ClientSection) ConfigurationManager.GetSection("system.serviceModel/client");
            var endpointAddress = clientSection.Endpoints.Cast<ChannelEndpointElement>()
                .Where(x => x.Name == endpointName)
                .Select(x => new EndpointAddress(LookupService(x.Address)))
                .FirstOrDefault();
            return endpointAddress;
        }
    }
}