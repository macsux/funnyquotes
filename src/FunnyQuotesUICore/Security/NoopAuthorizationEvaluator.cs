using Microsoft.AspNetCore.Authorization;

namespace FunnyQuotesUICore.Security
{
    public class NoopAuthorizationEvaluator : IAuthorizationEvaluator
    {
        public AuthorizationResult Evaluate(AuthorizationHandlerContext context)
        {
            return AuthorizationResult.Success();
        }
    }
}