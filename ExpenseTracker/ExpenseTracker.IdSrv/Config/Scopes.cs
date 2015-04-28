using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                    StandardScopes.Profile //used for profile information
                };

            return scopes;
        }
    }
}