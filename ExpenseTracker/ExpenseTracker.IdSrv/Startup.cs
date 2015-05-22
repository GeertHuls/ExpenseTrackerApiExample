using ExpenseTracker.Constants;
using ExpenseTracker.IdSrv.Config;
using Microsoft.Owin;
using Owin;
using System;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Configuration;

[assembly: OwinStartup(typeof(ExpenseTracker.IdSrv.Startup))]

namespace ExpenseTracker.IdSrv
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Well known url: https://localhost:44305/identity/.well-known/openid-configuration

            app.Map("/identity", idsrvApp =>
                {
                    idsrvApp.UseIdentityServer(
                        new IdentityServerOptions
                        {
                            SiteName = "Embedded IdentityServer",
                            IssuerUri = ExpenseTrackerConstants.IdSrvIssueUri,

                            Factory = InMemoryFactory.Create(
                                users: Users.Get(),
                                clients: Clients.Get(),
                                scopes: Scopes.Get()
                            ),
                            
                            SigningCertificate = LoadCertificate()
                        });
                });
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\Certificates\idsrv.local.pfx",
                AppDomain.CurrentDomain.BaseDirectory), "");
        }
    }
}