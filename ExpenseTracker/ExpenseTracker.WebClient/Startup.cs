using ExpenseTracker.Constants;
using ExpenseTracker.Repository.Helpers;
using ExpenseTracker.WebClient.Helpers;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web.Helpers;
using Thinktecture.IdentityModel.Client;

[assembly: OwinStartup(typeof(ExpenseTracker.WebClient.Startup))]

namespace ExpenseTracker.WebClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //This line prevent claim type to be mapped to dotnet's claim types, it resets the mapping dictionary,
            //ensure that no mapping occurs.
            //Second benefit is that the claim's names are much more readable.
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            //Use this claim instead of identifier or identity provider claim
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "unique_user_key";

            app.UseResourceAuthorization(new AuthorizationManager());

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                //ExpireTimeSpan = new TimeSpan()
                //SlidingExpiration = true
            });


            /***********************************************************************************************************
             * The authorization code flow in a nutshell:
             * ------------------------------------------
             * Client calls auth server
             * Auth server gives back a code to the client (web) app
             * The web app checks for CSRF like validation (probably using some sort of state parameter)
             * The client web app calls auth server back (using token endpoint) to finally get the access token (+optional refresh token)
             * The client web app calls resource server with that access token
             ***********************************************************************************************************/

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
                
                
                //There are 3 types of response types:
                // code: for authorization code
                // token: for an access token and to get userInfo
                // id_token: for identity token

                ResponseType = "code id_token token", // this depends on the flow you are using (implit vs authorization code flow)
                //the type above is configured for hybrid flow:
                //- we are working with identity so we require an id_token
                //- we also require the authorization code so that the flow can successfully complete

                //Example: authorization code flow requires authoriation code
                //If the client does not call the correct response type, the flow would not be able to successfully complete

                //Configure resource scopes:
                //Type of id scope are like a collections of claims
                //Profile matches to profile related claims such as given_name, family_name, picture, gender, etc...

                //Other scopes:
                // - address: country, postal code, etc... 
                // - ...
                //Check openid tech spec to get a full list of scope types

                Scope = "openid profile roles expensetrackerapi offline_access",
                //openid scope is a requirement for openid support
                //roles scope to request roles from id server
                //expensetrackerapi to allow access to the api

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    MessageReceived = async n =>
                    {
                        EndpointAndTokenHelper.DecodeAndWrite(n.ProtocolMessage.IdToken);
                        
                        //When 'token' is set in reponse type, the access token will contain the profile and openid scope
                        EndpointAndTokenHelper.DecodeAndWrite(n.ProtocolMessage.AccessToken);
                        //this allows us to access the user endpoint and retrieve the profile values
                        //given_name and family_name values will be available in userInfo
                        var userInfo = await EndpointAndTokenHelper
                            .CallUserInfoEndpoint(n.ProtocolMessage.AccessToken);

                        //Best practise:
                        //Put very basic information in the id_token,
                        //but return extended information through a seperate call to the userinfo endpoint.
                    },

                    //Using claims transformation
                    SecurityTokenValidated = async n =>
                    {
                        //Debug access token use: EndpointAndTokenHelper.DecodeAndWrite(n.ProtocolMessage.AccessToken)

                        var userInfo = await EndpointAndTokenHelper.CallUserInfoEndpoint(n.ProtocolMessage.AccessToken);

                        var givenNameClaim = new Claim(
                            Thinktecture.IdentityModel.Client.JwtClaimTypes.GivenName,
                            userInfo.Value<string>("given_name"));

                        var familyNameClaim = new Claim(
                            Thinktecture.IdentityModel.Client.JwtClaimTypes.FamilyName,
                            userInfo.Value<string>("family_name"));

                        var roles = userInfo.Value<JArray>("role").ToList();

                        var newIdentity = new ClaimsIdentity(
                           n.AuthenticationTicket.Identity.AuthenticationType,
                           Thinktecture.IdentityModel.Client.JwtClaimTypes.GivenName,
                           Thinktecture.IdentityModel.Client.JwtClaimTypes.Role);

                        newIdentity.AddClaim(givenNameClaim);
                        newIdentity.AddClaim(familyNameClaim);

                        foreach (var role in roles) //Add the roles to the claims:
                        {
                            newIdentity.AddClaim(new Claim(
                                Thinktecture.IdentityModel.Client.JwtClaimTypes.Role,
                                role.ToString()));
                        }

                        var issuerClaim = n.AuthenticationTicket.Identity
                            .FindFirst(Thinktecture.IdentityModel.Client.JwtClaimTypes.Issuer);
                        var subjectClaim = n.AuthenticationTicket.Identity
                            .FindFirst(Thinktecture.IdentityModel.Client.JwtClaimTypes.Subject);

                        newIdentity.AddClaim(new Claim("unique_user_key", //this claim is used as AntiForgery token
                            issuerClaim.Value + "_" + subjectClaim.Value)); //userId is composed of issuer and subject



                        //This is used for client credentials flow (server to server):
                        //var oAuth2Client = new OAuth2Client(
                        // new Uri(ExpenseTrackerConstants.IdSrvToken),
                        // "mvc_api",
                        // "secret");

                        //var response = oAuth2Client.RequestClientCredentialsAsync("expensetrackerapi").Result;
                        //EndpointAndTokenHelper.DecodeAndWrite(response.AccessToken)



                        //Add access token to list of cliams, next pass this access token to api on each call
                        //so that resource scope can fulfill requests. To do this add it to the bearer token
                        //in the http client headers.
                        //newIdentity.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken)); // this token is replaced by the refresh tokens below:


                        // use the authorization code to get a refresh token 
                        var tokenEndpointClient = new OAuth2Client(
                            new Uri(ExpenseTrackerConstants.IdSrvToken),
                            "mvc", "secret");

                        var tokenResponse = await tokenEndpointClient.RequestAuthorizationCodeAsync(
                            n.ProtocolMessage.Code, ExpenseTrackerConstants.ExpenseTrackerClient);

                        //tokenResponse.IdentityToken <-- This is the token from openid connect.
                        //  This extra special token was missing from the original naïve
                        //  OAuth authentication implementation.
                        //  This id token is meant for the client, its reponsibility
                        //  is now immediately validate this id token.
                        //  Inside this token are 4 important claims:
                        //  - Issuer (iss): the id server
                        //  - Subject (sub): identifies the user
                        //  - Audience (aud): specifies for who this token is for,
                        //      the client that initiated the request (this web client)
                        //  - Expiration (exp): these token must be short lived, they cannot be kept alive forever
                        //
                        //  The token is signed, and it must be validated at this point.
                        //  Ensure that all the players are legit.

                        //  Example how to validate id token:
                        //  https://github.com/IdentityServer/basic.identityserver.io/blob/master/BasicClient/Controllers/AccountController.cs#L128

                        newIdentity.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
                        newIdentity.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                        newIdentity.AddClaim(new Claim("expires_at",
                            DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));



                        n.AuthenticationTicket = new AuthenticationTicket(
                            newIdentity,
                            n.AuthenticationTicket.Properties);
                    },
                }
            });

        }

    }
}