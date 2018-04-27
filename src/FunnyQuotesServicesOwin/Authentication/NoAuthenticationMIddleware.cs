using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace FunnyQuotesServicesOwin.Authentication
{
    public class NoAuthenticationMiddleware : OwinMiddleware
    {
        public NoAuthenticationMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            
            var identity = new ClaimsPrincipal(new ClaimsIdentity("noauth"));
            context.Authentication.User = identity;
            Next.Invoke(context);
            return Task.CompletedTask;
        }
    }
}