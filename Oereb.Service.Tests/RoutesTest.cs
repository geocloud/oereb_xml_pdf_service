using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oereb.Service.Controllers;
using Oereb.Service.Tests.Helper;

namespace Oereb.Service.Tests
{
    [TestClass]
    public class RoutesTest
    {
        HttpConfiguration _config;

        public RoutesTest()
        {
            _config = new HttpConfiguration();

            //add existing routes
            WebApiConfig.Register(_config);

            _config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
           _config.EnsureInitialized();
        }
        
        [TestMethod]
        public void CheckRouteReport()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://www.dummy.ch/oereb/report/falvour");

            var routeTester = new RouteTester(_config, request);
            var controller = new ReportController();

            Assert.AreEqual(controller.GetType(), routeTester.GetControllerType());

            var actionName = ReflectionHelpers.GetMethodName((ReportController p) => p.Create("",false, false));
            Assert.AreEqual(actionName, routeTester.GetActionName());
        }
    }
}
