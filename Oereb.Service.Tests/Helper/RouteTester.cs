using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Oereb.Service.Tests.Helper
{
    public class RouteTester
    {
        public HttpConfiguration Config;
        public HttpRequestMessage Request;
        public IHttpRouteData RouteData;
        public IHttpControllerSelector ControllerSelector;
        public HttpControllerContext ControllerContext;

        public RouteTester(HttpConfiguration configuration, HttpRequestMessage request)
        {
            Config = configuration;
            Request = request;
            RouteData = Config.Routes.GetRouteData(Request);
            Request.Properties[HttpPropertyKeys.HttpRouteDataKey] = RouteData;
            ControllerSelector = new DefaultHttpControllerSelector(Config);
            ControllerContext = new HttpControllerContext(Config, RouteData, Request);
        }

        public string GetActionName()
        {
            if (ControllerContext.ControllerDescriptor == null)
            {
                GetControllerType();
            }

            var routeData = ControllerContext.RouteData.Values;

            //add project (simulate), because the handler MessageHandlerLevel isn't executed

            //if (ControllerContext.RouteData.Route.Handler is App_Start.MessageHandlerLevel)
            //{
            //    routeData.Add("project", "xxx");
            //}

            var actionSelector = new ApiControllerActionSelector();
            var descriptor = actionSelector.SelectAction(ControllerContext);
            return descriptor.ActionName;
        }

        public Type GetControllerType()
        {
            var descriptor = ControllerSelector.SelectController(Request);
            ControllerContext.ControllerDescriptor = descriptor;
            return descriptor.ControllerType;
        }
    }
}
