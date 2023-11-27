using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Text;
using System.Net;

namespace POSSUM.StandAloneApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static MqttHelper mqttHelper;
        public static bool isConnected = false;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //Initalize Mqtt
            mqttHelper = new MqttHelper();
        }
    }
}
