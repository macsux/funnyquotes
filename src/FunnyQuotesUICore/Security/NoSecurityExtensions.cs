using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FunnyQuotesUICore.Security
{
    public static class NoSecurityExtensions
    {
        public static AuthenticationBuilder AddNoSecurity(
            this AuthenticationBuilder builder)
        {
            return builder.AddScheme<AuthenticationSchemeOptions, NoSecurityAuthenticatoinHandler>("noauth", null);
        }

        public static void NoAuthorization(this IServiceCollection services)
        {
            services.Remove(services.FirstOrDefault(x => x.ServiceType == typeof(IAuthorizationEvaluator)));
            services.TryAdd(ServiceDescriptor.Transient<IAuthorizationEvaluator, NoopAuthorizationEvaluator>());

        }
    }
}
