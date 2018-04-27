using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Owin;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.Security.Authentication.CloudFoundry;

namespace FunnyQuotesServicesOwin.Authentication
{
    public static class CloudFoundryJwtBearerExtensions
    {
        public static void AddCloudFoundryJwtBearer(this IAppBuilder app, IConfiguration config)
        {
            var cloudFoundryOptions = new CloudFoundryJwtBearerOptions();
            var securitySection = config.GetSection(CloudFoundryDefaults.SECURITY_CLIENT_SECTION_PREFIX);
            securitySection.Bind(cloudFoundryOptions);

            SsoServiceInfo si = config.GetSingletonServiceInfo<SsoServiceInfo>();
            if (si == null)
                return;
            var jwtTokenUrl = si.AuthDomain + CloudFoundryDefaults.JwtTokenKey;
            var httpMessageHandler = CloudFoundryHelper.GetBackChannelHandler(cloudFoundryOptions.ValidateCertificates);
            var tokenValidationParameters = GetTokenValidationParameters(jwtTokenUrl, httpMessageHandler, cloudFoundryOptions.ValidateCertificates);
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    TokenValidationParameters = tokenValidationParameters,
                });
        }

        internal static TokenValidationParameters GetTokenValidationParameters(string keyUrl, HttpMessageHandler handler, bool validateCertificates)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidateLifetime = true,
                IssuerValidator = CloudFoundryTokenValidator.ValidateIssuer
            };


            var tkr = new CloudFoundryTokenKeyResolver(keyUrl, handler, validateCertificates);
            parameters.IssuerSigningKeyResolver = tkr.ResolveSigningKey;

            return parameters;
        }
    }
}