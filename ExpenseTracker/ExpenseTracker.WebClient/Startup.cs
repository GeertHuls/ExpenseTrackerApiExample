﻿using ExpenseTracker.Constants;
using ExpenseTracker.WebClient.Helpers;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(ExpenseTracker.WebClient.Startup))]

namespace ExpenseTracker.WebClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });


            //Open connect middleware with supports hybrid flow:
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = "mvc", //this name is defined in id srv: ExpenseTracker.IdSrv.Config.Clients class
                Authority = ExpenseTrackerConstants.IdSrv,
                RedirectUri = ExpenseTrackerConstants.ExpenseTrackerClient,
                SignInAsAuthenticationType = "Cookies",

                //Possibility to  define 3 types of responses:
                //code for authorization code
                //token for access token
                //id_token for an identity
                
                
                ResponseType = "code id_token", // this depends on the flow you are using (implit vs authorization code flow)
                //the type above is configured for hybrid flow:
                //- we are working with identity so we require an id_token
                //- we also require the authorization code so that the flow can successfully complete

                //Example: authorization code flow requires authoriation code
                //If the client does not call the correct response type, the flow would not be able to successfully complete

                Scope = "openid", //profile
                //openid scope is a requirement for openid support

                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    MessageReceived = async n =>
                    {
                        EndpointAndTokenHelper.DecodeAndWrite(n.ProtocolMessage.IdToken);
                    }
                }
            });

        }

    }
}