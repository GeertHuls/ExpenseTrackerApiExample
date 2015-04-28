using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ExpenseTracker.IdSrv.Startup))]

namespace ExpenseTracker.IdSrv
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}