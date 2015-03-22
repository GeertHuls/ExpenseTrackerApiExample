using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ExpenseTracker.Api.Startup))]

namespace ExpenseTracker.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWebApi(WebApiConfig.Register());
        }
    }
}