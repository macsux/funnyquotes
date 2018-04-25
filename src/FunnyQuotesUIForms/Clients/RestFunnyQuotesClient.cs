using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FunnyQuotesCommon;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUIForms.Clients
{
    public class RestFunnyQuotesClient : IFunnyQuoteService
    {
        private const string RANDOM_FunnyQuotes_URL = "http://FunnyQuotesServicesOwin/api/FunnyQuotes/random";
        private readonly DiscoveryHttpClientHandler _handler;

        public RestFunnyQuotesClient(IDiscoveryClient client)
        {
            _handler = new DiscoveryHttpClientHandler(client);
        }

        public async Task<string> GetCookieAsync()
        {
            var client = GetClient();

            var json = await client.GetStringAsync(RANDOM_FunnyQuotes_URL);
            var FunnyQuotes = JsonConvert.DeserializeObject<string>(json);
            return FunnyQuotes;
        }

        public string GetCookie()
        {
            throw new NotImplementedException();
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient(_handler, false);
            // distributed tracing headers
            var traceId = HttpContext.Current.Request.Headers["X-B3-TraceId"];
            if (traceId != null)
            {
                client.DefaultRequestHeaders.Add("X-B3-TraceId", traceId);
                client.DefaultRequestHeaders.Add("X-B3-ParentSpanId", HttpContext.Current.Request.Headers["X-B3-SpanId"]);
                client.DefaultRequestHeaders.Add("X-B3-SpanId", Guid.NewGuid().ToString());
            }

            return client;
        }
    }
}