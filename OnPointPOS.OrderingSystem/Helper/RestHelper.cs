using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
//using System.Web.Http;
using System.Xml;

namespace ML.Common
{
    public class RestHelper
    {
        public static XmlDocument GetXmlDocument(string strBaseAddress, string strUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                HttpResponseMessage response = client.GetAsync(strUrl).Result;

                string str = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(str);

                XmlDocument xml = new XmlDocument();
                xml.Load(response.Content.ReadAsStreamAsync().Result);

                return xml;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }


        public static XmlDocument PostAndGetXmlDocument(string strBaseAddress, string strUrl, HttpContent httpContent)
        {
            try
            {
                HttpClient client = new HttpClient();   
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                HttpResponseMessage response = client.PostAsync(strUrl, httpContent).Result;

                XmlDocument xml = new XmlDocument();
                xml.Load(response.Content.ReadAsStreamAsync().Result);

                return xml;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return null;
        }

        public static XmlReader GetXmlReader(string strBaseAddress, string strUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                HttpResponseMessage response = client.GetAsync(strUrl).Result;

                XmlReader xmlReader = XmlReader.Create(response.Content.ReadAsStreamAsync().Result);

                return xmlReader;
            }
            catch
            {
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JArray GetJsonArray(string strBaseAddress, string strUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(strUrl).Result;

                Newtonsoft.Json.Linq.JArray jarray = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jarray;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JObject GetJsonObject(string strBaseAddress, string strUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(strUrl).Result;

                Newtonsoft.Json.Linq.JObject obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return obj;
            }
            catch
            {
            }

            return null;
        }

        //public static Newtonsoft.Json.Linq.JObject GetJsonObject(string strBaseAddress, string strUrl)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(strBaseAddress);
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    HttpResponseMessage response = client.GetAsync(strUrl).Result;

        //    return (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
        //}


        public static Newtonsoft.Json.Linq.JObject PostAndGetJsonObject(string strBaseAddress, string strUrl, HttpContent httpContent)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.PostAsync(strUrl, httpContent).Result;

                Newtonsoft.Json.Linq.JObject obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return obj;
            }
            catch
            {
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JArray PostAndGetJsonArray(string strBaseAddress, string strUrl, HttpContent httpContent)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.PostAsync(strUrl, httpContent).Result;

                Newtonsoft.Json.Linq.JArray jarray = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jarray;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        //public static Newtonsoft.Json.Linq.JObject PostAndGetJsonObject(string strBaseAddress, string strUrl, HttpContent httpContent)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(strBaseAddress);
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    HttpResponseMessage response = client.PostAsync(strUrl, httpContent).Result;

        //    return (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
        //}

        public static Newtonsoft.Json.Linq.JObject PutAndGetJsonObject(string strBaseAddress, string strUrl, HttpContent httpContent)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.PutAsync(strUrl, httpContent).Result;

                Newtonsoft.Json.Linq.JObject obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return obj;
            }
            catch
            {
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JArray PutAndGetJsonArray(string strBaseAddress, string strUrl, HttpContent httpContent)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.PutAsync(strUrl, httpContent).Result;

                Newtonsoft.Json.Linq.JArray jarray = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jarray;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JObject DeleteAndGetJsonObject(string strBaseAddress, string strUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.DeleteAsync(strUrl).Result;

                Newtonsoft.Json.Linq.JObject obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return obj;
            }
            catch
            {
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JArray DeleteAndGetJsonArray(string strBaseAddress, string strUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.DeleteAsync(strUrl).Result;

                Newtonsoft.Json.Linq.JArray jarray = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jarray;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }


        //public static HttpResponseMessage GetHttpResponseMessage(string strBaseAddress, string strUrl)
        //{
        //    try
        //    {
        //        //int intPos = strBaseAddress.IndexOf("//");
        //        //intPos = strBaseAddress.IndexOf("/", intPos + 2);
        //        //string strUrlMid = strBaseAddress.Substring(intPos);

        //        if (string.IsNullOrEmpty(strUrl))
        //        {
        //            int intPosFirstDot = strBaseAddress.IndexOf('.');
        //            int intPosTopDomainEnd = strBaseAddress.Substring(intPosFirstDot).IndexOf('/');
        //            if (intPosTopDomainEnd > -1)
        //            {
        //                strUrl = strBaseAddress.Substring(intPosFirstDot + intPosTopDomainEnd);
        //                strBaseAddress = strBaseAddress.Substring(0, intPosFirstDot + intPosTopDomainEnd);
        //            }
        //        }

        //        HttpClient client = new HttpClient();
        //        client.BaseAddress = new Uri(strBaseAddress);
        //        HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                
        //        //httpResponseMessage = client.GetAsync(string.Format("{0}{1}", strUrlMid, strUrl)).Result;
        //        httpResponseMessage = client.GetAsync(strUrl).Result;

        //        return httpResponseMessage;

        //    }
        //    catch
        //    {
        //    }

        //    return null;
        //}

        //public static HttpResponseMessage PostAndGetHttpResponseMessage(string strBaseAddress, string strUrl, HttpContent httpContent)
        public static HttpResponseMessage PostAndGetHttpResponseMessage(string strUrl, HttpContent httpContent)
        {
            //new ML.Email.Email().SendDebug("PostAndGetHttpResponseMessage 1", string.Concat("strBaseAddress:", strBaseAddress, ", strUrl:", strUrl));

            try
            {
                //if (string.IsNullOrEmpty(strUrl))
                //{
                //    int intPosFirstDot = strBaseAddress.IndexOf('.');
                //    int intPosTopDomainEnd = strBaseAddress.Substring(intPosFirstDot).IndexOf('/');
                //    if (intPosTopDomainEnd > -1)
                //    {
                //        strUrl = strBaseAddress.Substring(intPosFirstDot + intPosTopDomainEnd);
                //        strBaseAddress = strBaseAddress.Substring(0, intPosFirstDot + intPosTopDomainEnd);

                //        //new ML.Email.Email().SendDebug("PostAndGetHttpResponseMessage 2", string.Concat("strBaseAddress:", strBaseAddress, ", strUrl", strUrl));
                //    }
                //}

                //HttpClient client = new HttpClient();
                //client.Timeout = new TimeSpan(0, 5, 0);
                //client.BaseAddress = new Uri(strBaseAddress);
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                //httpResponseMessage = client.PostAsync(strUrl, httpContent).Result;


                HttpClient client = new HttpClient();
                httpResponseMessage = client.PostAsync(strUrl, httpContent).Result;

                //new ML.Email.Email().SendDebug("ML.Common.Resthelper.PostAndGetHttpResponseMessage 3", string.Concat("httpResponseMessage.RequestMessage:", httpResponseMessage.RequestMessage.ToString(), ", httpResponseMessage.StatusCode:", httpResponseMessage.StatusCode.ToString(), ", httpResponseMessage.ReasonPhrase:", httpResponseMessage.ReasonPhrase.ToString(), "httpResponseMessage.Content.ReadAsStringAsync().Result:", httpResponseMessage.Content.ReadAsStringAsync().Result));

                return httpResponseMessage;
            }
            catch(Exception ex)
            {
                //new ML.Email.Email().SendDebug("ML.Common.Resthelper.PostAndGetHttpResponseMessage 4", string.Format("Message:{0}, StackTrace:{1}", ex.Message, ex.StackTrace));
            }

            return null;
        }

        public static HttpResponseMessage GetHttpResponseMessage(string strBaseAddress, string strUrl)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                httpResponseMessage = client.GetAsync(strUrl).Result;

                return httpResponseMessage;
            }
            catch (Exception ex)
            {
               // new ML.Email.Email().SendDebug("ML.Common.Resthelper.GetHttpResponseMessage", string.Format("Message:{0}, StackTrace:{1}", ex.Message, ex.StackTrace));
            }

            return null;
        }

        public static HttpResponseMessage PostAndGetHttpResponseMessage(string strUrl, HttpContent httpContent, string strUser, string strPassword)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", strUser, strPassword))));
                
                httpResponseMessage = client.PostAsync(strUrl, httpContent).Result;

                return httpResponseMessage;
            }
            catch (Exception ex)
            {
               // new ML.Email.Email().SendDebug("ML.Common.Resthelper.PostAndGetHttpResponseMessage 5", string.Format("Message:{0}, StackTrace:{1}", ex.Message, ex.StackTrace));
            }

            return null;
        }

        public static HttpResponseMessage GetAndGetHttpResponseMessage(string strUrl, string strUser, string strPassword)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", strUser, strPassword))));

                httpResponseMessage = client.GetAsync(strUrl).Result;

                return httpResponseMessage;
            }
            catch (Exception ex)
            {
                //new ML.Email.Email().SendDebug("ML.Common.Resthelper.PostAndGetHttpResponseMessage 5", string.Format("Message:{0}, StackTrace:{1}", ex.Message, ex.StackTrace));
            }

            return null;
        }

        //public static string Post(string strUrl, System.Collections.Specialized.NameValueCollection nameValueCollection)
        //{
        //    WebClient webClient = new WebClient();
        //    byte[] responseArray = webClient.UploadValues(strUrl, "POST", nameValueCollection);
        //    webClient.Dispose();
        //    return Encoding.UTF8.GetString(responseArray);
        //}

        //public static HttpResponseMessage PostFileAndGetHttpResponseMessage(string strBaseAddress, string strUrl, string strFile, string strFileName)
        //{
        //    try
        //    {
        //        HttpClient client = new HttpClient();
        //        client.BaseAddress = new Uri(strBaseAddress);

        //        MultipartFormDataContent content = new MultipartFormDataContent();
        //        System.Net.Http.ByteArrayContent byteArrayContent = new ByteArrayContent(System.IO.File.ReadAllBytes(strFile));
        //        byteArrayContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        //        {
        //            FileName = strFileName
        //        };
        //        content.Add(byteArrayContent);
        //        return client.PostAsync(strUrl, content).Result;
        //    }
        //    catch
        //    {
        //    }

        //    return null;
        //}

        public static HttpResponseMessage PostImageAndGetHttpResponseMessage(string strBaseAddress, string strUrl, Image image, string strFileName)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);

                MultipartFormDataContent content = new MultipartFormDataContent();
                System.Net.Http.ByteArrayContent byteArrayContent = new ByteArrayContent(ML.Common.StringHelper.ImageToByteArray(image));
                byteArrayContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = strFileName
                };
                content.Add(byteArrayContent);
                return client.PostAsync(strUrl, content).Result;
            }
            catch
            {
            }

            return null;
        }

        public static HttpResponseMessage PostMultipartFormDataContentAndGetHttpResponseMessage(string strBaseAddress, string strUrl, MultipartFormDataContent content)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                return client.PostAsync(strUrl, content).Result;
            }
            catch
            {
            }

            return null;
        }

        public static XmlDocument PostMultipartFormDataContentAndGetXml(string strBaseAddress, string strUrl, MultipartFormDataContent content)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                HttpResponseMessage response = client.PostAsync(strUrl, content).Result;

                XmlDocument xml = new XmlDocument();
                xml.Load(response.Content.ReadAsStreamAsync().Result);

                return xml;
            }
            catch (Exception  ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JObject PostMultipartFormDataContentAndGetJsonObject(string strBaseAddress, string strUrl, MultipartFormDataContent content)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.PostAsync(strUrl, content).Result;

                Newtonsoft.Json.Linq.JObject jobject = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jobject;
            }
            catch
            {
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JArray PostMultipartFormDataContentAndGetJsonArray(string strBaseAddress, string strUrl, MultipartFormDataContent content)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.PostAsync(strUrl, content).Result;

                Newtonsoft.Json.Linq.JArray jarray = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jarray;
            }
            catch
            {
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JObject PutMultipartFormDataContentAndGetJsonObject(string strBaseAddress, string strUrl, MultipartFormDataContent content)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.PutAsync(strUrl, content).Result;

                Newtonsoft.Json.Linq.JObject jobject = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jobject;
            }
            catch
            {
            }

            return null;
        }

        public static Newtonsoft.Json.Linq.JArray PutMultipartFormDataContentAndGetJsonArray(string strBaseAddress, string strUrl, MultipartFormDataContent content)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(strBaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.PutAsync(strUrl, content).Result;

                Newtonsoft.Json.Linq.JArray jarray = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                return jarray;
            }
            catch
            {
            }

            return null;
        }



        //Newtonsoft.Json.Linq.JObject json = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
        
        //System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer();

        public static HttpResponseMessage ConvertStream(MemoryStream ms)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(ms);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }


    }
}
