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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesBasicLinux.Clients
{
    public class RestFunnyQuotesClient : IFunnyQuoteService
    {
        private readonly GetQuoteCommand _getQuoteCommand;
        private readonly IDiscoveryClient _discoveryClient;

        public RestFunnyQuotesClient(GetQuoteCommand getQuoteCommand, IDiscoveryClient discoveryClient)
        {
            _getQuoteCommand = getQuoteCommand;
            _discoveryClient = discoveryClient;
        }


        public string GetQuote()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetQuoteAsync()
        {
            var cmd = _getQuoteCommand;
            return await cmd.ExecuteAsync();
        }


        public class GetQuoteCommand : HystrixCommand<string>
        {
            private readonly HttpClient _httpClient;
            private readonly FunnyQuotesConfiguration _config;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly ILogger<GetQuoteCommand> _logger;

            private static IHystrixCommandOptions _options =
                new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("Core.RandomQuote"), HystrixCommandKeyDefault.AsKey("Core.RandomQuote"));

            public GetQuoteCommand(HttpClient httpClient, 
                IOptions<FunnyQuotesConfiguration> config, 
                IHttpContextAccessor httpContextAccessor, 
                ILogger<GetQuoteCommand> logger) : base(_options)
            {
                
                _httpClient = httpClient;
                _config = config.Value;
                _httpContextAccessor = httpContextAccessor;
                _logger = logger;
            }

            protected override async Task<string> RunAsync()
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

            protected override string RunFallback()
            {
                _logger.LogTrace("Invoking GetQuote Fallback");
                if (IsResponseTimedOut)
                {
                    _logger.LogWarning("Circuit is experiencing a service degradation due to timeout");
                }
                return _config.FailedMessage;
            }

        }
    }
}