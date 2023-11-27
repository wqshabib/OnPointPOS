using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Web.Http;
using System.Xml;
using System.Configuration;

namespace POSSUM.Api.Controllers
{
    public class CallbackController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Callback(string code, string scope, string state)
        {
            try
            {
                string vismaSettingFilePath = ConfigurationManager.AppSettings["VismaSettingFilePath"].ToString();
                string vismaTokenFilePath = ConfigurationManager.AppSettings["VismaTokenFilePath"].ToString();
                string vismaTokenFileName = ConfigurationManager.AppSettings["VismaTokenFileName"].ToString();
                string vismaServiceName = ConfigurationManager.AppSettings["VismaServiceName"].ToString();

                startStopService(vismaServiceName, false, true);

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(vismaSettingFilePath);

                XmlNodeList authNodes = xmlDocument.SelectNodes("//AppSetting/AuthCode");
                XmlNode authXmlNode = authNodes.Item(0);
                authXmlNode.InnerText = code;

                xmlDocument.Save(vismaSettingFilePath);

                if(File.Exists(Path.Combine(vismaTokenFilePath, vismaTokenFileName))) 
                    File.Delete(Path.Combine(vismaTokenFilePath, vismaTokenFileName));

                startStopService(vismaServiceName, true, false);

                string message = "Code: " + code + ", Scope: " + scope + ", State: " + state;

                string json = JsonConvert.SerializeObject(message);
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                return httpResponseMessage;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        private void startStopService(string serviceName, bool isStart, bool isStop)
        {
            try
            {
                ServiceController serviceController = new ServiceController();
                serviceController.ServiceName = serviceName;

                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    if (isStart)
                    {
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running);
                    }
                }
                else if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    if (isStop)
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}