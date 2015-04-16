using ExpenseTracker.Constants;
using System;
using System.Net.Http;

namespace ExpenseTracker.WebClient.Helpers
{
    public static class ExpenseTrackerHttpClient
    {

        public static HttpClient GetClient()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(ExpenseTrackerConstants.ExpenseTrackerAPI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}