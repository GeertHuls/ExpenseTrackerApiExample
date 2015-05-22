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
                    }
                };

            return scopes;
        }
    }
}