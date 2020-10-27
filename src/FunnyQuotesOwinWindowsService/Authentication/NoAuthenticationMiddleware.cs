using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace FunnyQuotesOwinWindowsService.Authentication
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