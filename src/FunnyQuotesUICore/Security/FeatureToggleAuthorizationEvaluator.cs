using FunnyQuotesCommon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FunnyQuotesUICore.Security
{
    public class FeatureToggleAuthorizationEvaluator : IAuthorizationEvaluator
    {
        private readonly IOptionsMonitor<FunnyQuotesConfiguration> _config;
        private IAuthorizationEvaluator _defaultEvaulator = new DefaultAuthorizationEvaluator();

        public FeatureToggleAuthorizationEvaluator(IOptionsMonitor<FunnyQuotesConfiguration> config)
        {
            _config = config;
        }

        public AuthorizationResult Evaluate(AuthorizationHandlerContext context)
        {
            if (_config.CurrentValue.EnableSecurity)
                return _defaultEvaulator.Evaluate(context);
            else
                return AuthorizationResult.Success();
        }
    }
}