using System.Data.Entity.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using Microsoft.Extensions.Logging;
#pragma warning disable 1998

namespace FunnyQuotesServicesOwin
{
    public class LoggerExceptionFilterAttribute : IAutofacExceptionFilter
    {
        private readonly ILoggerFactory _loggerFactory;

        public LoggerExceptionFilterAttribute(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var logger = _loggerFactory.CreateLogger(actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName);
            var exception = actionExecutedContext.Exception;
            if(exception != null)
                logger.LogError(exception, exception.Message);
        }
    }
}