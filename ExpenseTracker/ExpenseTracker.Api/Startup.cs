using ExpenseTracker.API.Helpers;
using ExpenseTracker.Constants;
using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityServer.AccessTokenValidation;

[assembly: OwinStartup(typeof(ExpenseTracker.Api.Startup))]

namespace ExpenseTracker.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseResourceAuthorization(new AuthorizationManager());

            //Ensure that id server only accepts access_token created on this server
            app.UseIdentityServerBearerTokenAuthentication(
                new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = ExpenseTrackerConstants.IdSrv,
                RequiredScopes = new[] { "expensetrackerapi" } //This is the resource scope defined in the scopes.cs file
            });
            

            app.UseWebApi(WebApiConfig.Register());
        }
    }
}