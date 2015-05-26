using ExpenseTracker.Constants;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace ExpenseTracker.IdSrv.Config
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client
                {
                    Enabled = true,
                    ClientName = "ExpenseTracker MVC Client (Hybrid Flow)",
                    ClientId = "mvc",
                    Flow = Flows.Hybrid, //hybrid flow is mix between implit + authorization code
                    RequireConsent = true, //ensures that user will see approve consent screen
                    //(in this example we only ask for openid so the app only ask to allow personal identity)

                    RedirectUris = new List<string>
                        {
                            ExpenseTrackerConstants.ExpenseTrackerClient
                        },

                    AccessTokenLifetime = 60, //refresh token of 60 seconds
                                             //the hybrid slow does not directly retrun refresh token
                                             //instead we'll have to ask for it using the token end point
                                             //passing in the authorization code

                    ClientSecrets = new List<ClientSecret>() //this client secret will be used
                                            //to call for refresh token demand (server to server using code authorization
                    {
                        new ClientSecret("secret".Sha256())
                    }
                },

                new Client
                {
                    ClientName = "Expense Tracker Native Client (Implicit Flow)",
                    Enabled = true,
                    ClientId = "native", 
                    Flow = Flows.Implicit,
                    RequireConsent = true,
                                        
                    RedirectUris = new List<string>
                    {
                        ExpenseTrackerConstants.ExpenseTrackerMobile
                    },

                    //This WP client will only support a subset of scopes.
                    //If no restrictions are set, then all previous scopes are used.
                    ScopeRestrictions = new List<string>
                    { 
                        Thinktecture.IdentityServer.Core.Constants.StandardScopes.OpenId, 
                        "roles",
                        "expensetrackerapi" //Also include the resource scope for wp client
                    }
                },

                new Client //The client credential flow is only usefull in server to server
                           //communication, because a secret has to be stored on 
                           //on server. This flow also does not have an identity.
                {
                    Enabled = true,
                    ClientName = "ExpenseTracker MVC Client API Access (Client Credentials Flow)",
                    ClientId = "mvc_api",
                    ClientSecrets = new List<ClientSecret>() {new ClientSecret("secret".Sha256())},               
                    Flow = Flows.ClientCredentials
                }
            };
        }
    }
}