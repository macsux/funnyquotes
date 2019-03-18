using System;
using System.ServiceModel;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
            try
            {
                var endpointAddress = _dicoveryAddressResolver.GetEndpointAddress("FunnyQuoteServiceWcf");

                Console.WriteLine("URI: " + endpointAddress.Uri.ToString());
                var channelFactory = new ChannelFactory<IFunnyQuoteService>("FunnyQuoteServiceWcf", endpointAddress);
                var result = channelFactory.CreateChannel().GetQuoteAsync().Result;

                Console.WriteLine("RESULT: " + result);
                return result;
            }
            catch (FaultException ex)
            {
                Console.WriteLine("FAULT: " + ex.ToString());

                return null;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("AGGEX: " + ex.ToString());
                Console.WriteLine("INNEREX: " + ex.InnerException.ToString());

                foreach (var x in ex.InnerExceptions)
                {
                    Console.WriteLine("INNNER: " + x.ToString());
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
                return null;
            }
        }

        public string GetCookieFallback() => _config.Value.FailedMessage;
        

        public string GetQuote()
        {
            throw new NotImplementedException();
        }
    }
}