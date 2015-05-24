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
                    Flow = Flows.Hybrid,
                    RequireConsent = true, //ensures that user will see approve consent screen
                    //(in this example we only ask for openid so the app only ask to allow personal identity)

                    RedirectUris = new List<string>
                        {
                            ExpenseTrackerConstants.ExpenseTrackerClient
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
                }
            };
        }
    }
}