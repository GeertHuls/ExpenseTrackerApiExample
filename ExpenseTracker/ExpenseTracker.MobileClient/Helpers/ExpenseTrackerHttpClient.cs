﻿using ExpenseTracker.Constants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
 

namespace ExpenseTracker.MobileClient.Helpers
{

    public static class ExpenseTrackerHttpClient
    {

        private static HttpClient currentClient = null;

        public static HttpClient GetClient()
        {
            if (currentClient == null)
            {
                var accessToken = App.ExpenseTrackerIdentity.Claims.First
                    (c => c.Name == "access_token").Value;

                currentClient = new HttpClient(new Marvin.HttpCache.HttpCacheHandler()
                {
                    InnerHandler = new HttpClientHandler()
                });

                currentClient.SetBearerToken(accessToken); //Set the access token to each http call as a bearertoken

                currentClient.BaseAddress = new Uri(ExpenseTrackerConstants.ExpenseTrackerAPI);

                currentClient.DefaultRequestHeaders.Accept.Clear();
                currentClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
 
            }

            return currentClient;
        }
         
    }

     
}