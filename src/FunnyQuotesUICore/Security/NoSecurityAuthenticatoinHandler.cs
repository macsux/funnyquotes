using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunnyQuotesUICore.Security
{
    public class NoSecurityDefaults
    {
        public const string AuthenticationScheme = "noauth";
    }
    public class NoSecurityAuthenticatoinHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public NoSecurityAuthenticatoinHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = new ClaimsPrincipal(new GenericIdentity("user"));
            var ticket = new AuthenticationTicket(principal, "noauth");
            Context.User = principal;
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
