using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace ExpenseTracker.IdSrv.Config
{
    public static class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            var scopes = new List<Scope>()
                {
                    //identity scopes
                    StandardScopes.OpenId, //to support identity tokens, use OpenId connect
                    StandardScopes.Profile, //used for profile information
                    new Scope //Add role scope so that client can demand it to be included in the token
                    {
                        Enabled = true,
                        Name = "roles",
                        DisplayName = "Roles",
                        Description = "The roles you belong to.",
                        Type = ScopeType.Identity,
                        Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim("role")
                        }
                    },

                    new Scope
                    {
                        Name = "expensetrackerapi",
                        DisplayName = "ExpenseTracker API Scope",
                        Type = ScopeType.Resource, //This scope prevents public access so that only authorized client
                                                   //can access the expense tracker api
                        Emphasize = false,
                        Enabled = true,
                        //IncludeAllClaimsForUser = true //This will send all claims (eg role claims) back to api
                                                         //We don't do this because we only send the claims that are
                                                         //absolutely required.
                        Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim("role") //Only send the role claims
                        }

                    },

                    new Scope //There are no roles with the client credential flow since there is no user
                    {
                        Name = "expensetrackerapi",
                        DisplayName = "ExpenseTracker API Scope",
                        Type = ScopeType.Resource,
                        Emphasize = false,
                        Enabled = true
                    },

                    StandardScopes.OfflineAccess //to support refresh access tokens

                };

            return scopes;
        }
    }
}