﻿using ExpenseTracker.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                }
            };
        }
    }
}