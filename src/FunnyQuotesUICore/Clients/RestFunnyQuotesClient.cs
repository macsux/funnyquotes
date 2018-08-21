using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUICore.Clients
{
    public class RestFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptionsSnapshot<FunnyQuotesConfiguration> _config;
        private const string RANDOM_FunnyQuotes_URL = "http://FunnyQuotesServicesOwin/api/FunnyQuotes/random";
        private readonly DiscoveryHttpClientHandler _handler;

        public RestFunnyQuotesClient(IHttpContextAccessor httpContextAccessor, IDiscoveryClient client, IOptionsSnapshot<FunnyQuotesConfiguration> config)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _handler = new DiscoveryHttpClientHandler(client);
            
        }


        public string GetQuote()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetQuoteAsync()
        {
            var options = new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("Core.RandomQuote"), HystrixCommandKeyDefault.AsKey("Core.RandomQuote"));
            var cmd = new HystrixCommand<string>(options,
                run: GetCookieRun,
                fallback: GetCookieFallback);
            return await cmd.ExecuteAsync();
        }

        public string GetCookieRun()
        {
            try
            {
                var client = GetClient().Result;

                var json = client.GetStringAsync(RANDOM_FunnyQuotes_URL).Result;
                var funnyQuote = JsonConvert.DeserializeObject<string>(json);
                return funnyQuote;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }

        }

        public string GetCookieFallback() => _config.Value.FailedMessage;

        private async Task<HttpClient> GetClient()
        {
            var client = new HttpClient(_handler, false);
            // distributed tracing headers
            if(_config.Value.EnableSecurity)
            { 
                var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                if (token != null)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            }
            var traceId = _httpContextAccessor.HttpContext.Request.Headers["X-B3-TraceId"].FirstOrDefault();
            if (traceId != null)
            {
                client.DefaultRequestHeaders.Add("X-B3-TraceId", traceId);
                client.DefaultRequestHeaders.Add("X-B3-ParentSpanId", _httpContextAccessor.HttpContext.Request.Headers["X-B3-SpanId"].FirstOrDefault());
                client.DefaultRequestHeaders.Add("X-B3-SpanId", Guid.NewGuid().ToString());
            }

            return client;
        }
    }
}