using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Oereb.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
               name: "GetPdfFromXml",
               routeTemplate: "report/{flavour}",
               defaults: new { controller = "report", action = "getpdffromxml" }
           );
        }
    }
}
