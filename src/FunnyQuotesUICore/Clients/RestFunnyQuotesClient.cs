using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
// using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Discovery;

namespace FunnyQuotesUICore.Clients
{
    public class RestFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly IDiscoveryClient _discoveryClient;
        private readonly HttpClient _httpClient;
        private readonly FunnyQuotesConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        public RestFunnyQuotesClient(IDiscoveryClient discoveryClient, HttpClient httpClient, 
            IOptions<FunnyQuotesConfiguration> config, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<RestFunnyQuotesClient> logger)
        {
            _discoveryClient = discoveryClient;
            _httpClient = httpClient;
            _config = config.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public string GetQuote()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetQuoteAsync()
        {
            _logger.LogTrace("Invoking GetQuote Run");
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "random");

                if (_config.EnableSecurity)
                {
                    var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                    if (token != null)
                        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

                }

                var response = await _httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var funnyQuote = JsonConvert.DeserializeObject<string>(json);
                return funnyQuote;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
            
        }
      
    }
}