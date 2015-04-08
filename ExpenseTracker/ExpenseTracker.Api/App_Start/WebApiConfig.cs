using System.Web.Http;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;

namespace ExpenseTracker.Api
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Register()
        {
            var config = new HttpConfiguration();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(name: "DefaultRouting",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(
                new MediaTypeHeaderValue("application/json-patch+json"));

            config.Formatters.JsonFormatter.SerializerSettings.Formatting
                = Newtonsoft.Json.Formatting.Indented;

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver
                = new CamelCasePropertyNamesContractResolver();

            config.MessageHandlers.Add(new CacheCow.Server.CachingHandler(config));
            /// Example:
            /// 1) Get http://localhost:679/api/expensegroups/1 + note the etag in the response header
            /// 
            /// 2) Get http://localhost:679/api/expensegroups/1 +
            /// User-Agent: Fiddler
            /// Host: localhost:679
            /// If-None-Match: W/"5d1dd8af7e2c4885a572020ebbe3edaa"
            /// 
            /// Reponse = 412 (Not modified)

            return config;
        }
    }
}
