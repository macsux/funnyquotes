using System;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesUICore.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Steeltoe.Security.Authentication.CloudFoundry;

namespace FunnyQuotesUICore
{
    public class FeatureToggleAuthenticationSchemeProvider : AuthenticationSchemeProvider
    {
        private readonly IOptionsMonitor<FunnyQuotesConfiguration> _appConfig;

        public FeatureToggleAuthenticationSchemeProvider(
            IOptions<AuthenticationOptions> options,
            IOptionsMonitor<FunnyQuotesConfiguration> appConfig)
            : base(options)
        {
            _appConfig = appConfig;
        }

        private async Task<AuthenticationScheme> GetRequestSchemeAsync()
        {
            if (_appConfig.CurrentValue.EnableSecurity)
                return await GetSchemeAsync(CloudFoundryDefaults.AuthenticationScheme);
            else
                return await GetSchemeAsync(NoSecurityDefaults.AuthenticationScheme);
        }

        public override async Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync() =>
            await GetRequestSchemeAsync() ??
            await base.GetDefaultAuthenticateSchemeAsync();

        public override async Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync() =>
            await GetRequestSchemeAsync() ??
            await base.GetDefaultChallengeSchemeAsync();

        public override async Task<AuthenticationScheme> GetDefaultForbidSchemeAsync() =>
            await GetRequestSchemeAsync() ??
            await base.GetDefaultForbidSchemeAsync();

        public override async Task<AuthenticationScheme> GetDefaultSignInSchemeAsync() =>
            await GetRequestSchemeAsync() ??
            await base.GetDefaultSignInSchemeAsync();

        public override async Task<AuthenticationScheme> GetDefaultSignOutSchemeAsync() =>
            await GetRequestSchemeAsync() ??
            await base.GetDefaultSignOutSchemeAsync();
    }
}