using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FortunesCommon;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;

namespace FortunesUIForms.Clients
{
    public class RestFortuneClient : IFortuneService
    {
        private const string RANDOM_FORTUNE_URL = "http://FortunesServicesOwin/api/fortunes/random";
        private readonly DiscoveryHttpClientHandler _handler;

        public RestFortuneClient(IDiscoveryClient client)
        {
            _handler = new DiscoveryHttpClientHandler(client);
        }

        public async Task<string> GetCookieAsync()
        {
            var client = GetClient();

            var json = await client.GetStringAsync(RANDOM_FORTUNE_URL);
            var fortune = JsonConvert.DeserializeObject<string>(json);
            return fortune;
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