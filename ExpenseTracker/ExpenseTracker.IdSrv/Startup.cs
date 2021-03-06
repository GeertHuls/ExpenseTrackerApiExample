﻿using ExpenseTracker.Constants;
using ExpenseTracker.IdSrv.Config;
using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
                            SigningCertificate = LoadCertificate(),

                            Factory = InMemoryFactory.Create(
                                users: Users.Get(),
                                clients: Clients.Get(),
                                scopes: Scopes.Get()
                            ),

                            AuthenticationOptions = new AuthenticationOptions
                            {
                                IdentityProviders = ConfigureIdentityProviders
                            }

                        });
                });
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                AuthenticationType = "Facebook",
                Caption = "Sign-in with Facebook",
                SignInAsAuthenticationType = signInAsType,
                AppId = "378152562385126",
                AppSecret = "8d572ccbc5a42153046e0b8052081c58",


                //Perform claims transformation (on id server) to map face book claims to our own claims:
                Provider = new FacebookAuthenticationProvider()
                {
                    OnAuthenticated = context =>
                    {
                        //Facebook return a lastname and firstname claims (see fb docs)
                        JToken lastName, firstName;
                        if (context.User.TryGetValue("last_name", out lastName))
                        {
                            context.Identity.AddClaim(new System.Security.Claims.Claim(
                                Thinktecture.IdentityServer.Core.Constants.ClaimTypes.FamilyName,
                                lastName.ToString()));
                        }

                        if (context.User.TryGetValue("first_name", out firstName))
                        {
                            context.Identity.AddClaim(new System.Security.Claims.Claim(
                                Thinktecture.IdentityServer.Core.Constants.ClaimTypes.GivenName,
                                firstName.ToString()));
                        }

                        //Facebook authenticated user get all roles for our app:
                        context.Identity.AddClaim(new System.Security.Claims.Claim("role", "WebReadUser"));
                        context.Identity.AddClaim(new System.Security.Claims.Claim("role", "WebWriteUser"));

                        context.Identity.AddClaim(new System.Security.Claims.Claim("role", "MobileReadUser"));
                        context.Identity.AddClaim(new System.Security.Claims.Claim("role", "MobileWriteUser"));

                        return Task.FromResult(0);
                    }
                }
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