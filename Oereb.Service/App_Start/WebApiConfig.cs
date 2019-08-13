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
                name: "ReportCreation",
                routeTemplate: "oereb/report/create",
                defaults: new { controller = "report", action = "create" }
            );

            config.Routes.MapHttpRoute(
                name: "ReportForm",
                routeTemplate: "oereb/report/form",
                defaults: new { controller = "report", action = "form" }
            );

            config.Routes.MapHttpRoute(
                name: "GetVersion",
                routeTemplate: "oereb/report/getVersion",
                defaults: new { controller = "report", action = "getVersion" }
            );
        }
    }
}
