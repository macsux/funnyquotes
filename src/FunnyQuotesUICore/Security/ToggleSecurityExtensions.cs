using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FunnyQuotesUICore.Security
{
    public static class ToggleSecurityExtensions
    {
        public static AuthenticationBuilder Toggleable(this AuthenticationBuilder builder)
        {
            // builder.Services.Remove(builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IAuthorizationEvaluator)));
            // builder.Services.TryAdd(ServiceDescriptor.Transient<IAuthorizationEvaluator, FeatureToggleAuthorizationEvaluator>());
            builder.Services.AddSingleton<IAuthorizationEvaluator, FeatureToggleAuthorizationEvaluator>();
            builder.AddScheme<AuthenticationSchemeOptions, NoSecurityAuthenticatoinHandler>(NoSecurityDefaults.AuthenticationScheme, null);
            builder.Services.AddSingleton<IAuthenticationSchemeProvider, FeatureToggleAuthenticationSchemeProvider>();
            return builder;
        }

    }
}
