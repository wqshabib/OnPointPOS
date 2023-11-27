using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class APIRedirect
    {
        public HttpResponseMessage GetRequst(string url)
        {


            string MOBLINK_BASE_URL = ConfigurationManager.AppSettings["Mofr_BASE_URL"];
            HttpClient client = new HttpClient(); 
            client.Timeout = new TimeSpan(0, 5, 0);
            client.BaseAddress = new Uri(MOBLINK_BASE_URL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));



            try
            {
                return client.GetAsync(url).Result;

            }
            catch (Exception e)
            {
                var response = new HttpResponseMessage();
                response.ReasonPhrase = "Error in API Call" + e.Message;
                return response;
            }

        }

        public HttpResponseMessage PostRequst(string url,MultipartContent mpc)
        {


            string MOBLINK_BASE_URL = ConfigurationManager.AppSettings["Mofr_BASE_URL"];
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 5, 0);
            client.BaseAddress = new Uri(MOBLINK_BASE_URL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));



            try
            {
                return client.PostAsync(url, mpc).Result;

            }
            catch (Exception e)
            {
                var response = new HttpResponseMessage();
                response.ReasonPhrase = "Error in API Call" + e.Message;
                return response;
            }

        }

    }

}