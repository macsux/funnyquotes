using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;

namespace FunnyQuotesServicesOwin.Authentication
{
    public class NoopAuthorizationEvaluator : IAuthorizationEvaluator
    {
        public AuthorizationResult Evaluate(AuthorizationHandlerContext context)
        {
            return AuthorizationResult.Success();
        }
    }
}