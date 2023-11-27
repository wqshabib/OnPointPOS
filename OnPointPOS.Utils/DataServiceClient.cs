using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net;

namespace POSSUM.Utils
{
    public class DataServiceClient
    {

        string _baseAddress = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
        string access_Token = "";
        DateTime LAST_EXECUTED_DATETIME = DateTime.Now;
        string TerminalId = ConfigurationManager.AppSettings["TerminalId"];

        public DataServiceClient(string baseUrl, string user, string password, bool isCyrano = false)
        {
            if (!string.IsNullOrEmpty(baseUrl))
                _baseAddress = baseUrl;
            if (isCyrano)
                Request_Session_TokenCyrano(user, password);
            else
                Request_Session_Token(user, password);
        }
        private void Request_Session_Token(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "grant_type", "password" ),
                            new KeyValuePair<string, string>( "userName", userName ),
                            new KeyValuePair<string, string> ( "password", password )
                        };
            var content = new FormUrlEncodedContent(pairs);
            var data = JsonConvert.SerializeObject(new
            {
                grant_type = "password",
                userName = userName,
                password = password
            });
            //using (var client = new WebClient { UseDefaultCredentials = true })
            //{
            //    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            //    client.BaseAddress = _baseAddress;

            //    var result = client.UploadData("Token", "POST", Encoding.UTF8.GetBytes(data));
            //    var objects = result.Cast<Token>().First();
            //}
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response =
                    client.PostAsync(_baseAddress + "Token", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var token = JsonConvert.DeserializeObject<Token>(json);
                    access_Token = token.AccessToken;
                }
                else
                {
                    Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }

            }
        }

        private void Request_Session_TokenCyrano(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                        {
                            //new KeyValuePair<string, string>( "grant_type", "password" ),
                            new KeyValuePair<string, string>( "UserName", userName ),
                            new KeyValuePair<string, string> ( "Password", password )
                        };
            var contentl = new FormUrlEncodedContent(pairs);
            var data = JsonConvert.SerializeObject(new
            {
                UserName = userName,
                Password = password
            });

            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response =
                    client.PostAsync(_baseAddress + "Account", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var token = JsonConvert.DeserializeObject<List<dynamic>>(json);
                    access_Token = token.First()["Token"];
                }
                else
                {
                    Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }

            }
        }



        internal HttpResponseMessage PostProduct(Product product)
        {
            try
            {
                var productStr = JsonConvert.SerializeObject(product);
                Log.WriteLog("Posting Product with detail = " + productStr);
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex);
            }


            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<Product>("Product/PostProduct", product).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Sync Product Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;
        }


        public ProductData GetProducts(Guid terminalId, DateTime LastExecutedDate, DateTime executedDate)
        {
            ProductData productData = new ProductData();

            try
            {
                Log.WriteLog("Calling GetProducts with _baseAddress=" + _baseAddress + ", terminal = " + terminalId + ", LastExecutedDate=" + LastExecutedDate + ", executedDate=" + executedDate + ", and access_Token=" + access_Token);
                Dates dates = new Dates { CurrentDate = DateTime.Now, LastExecutedDate = LastExecutedDate, TerminalId = terminalId };
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = new Uri(_baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:

                var url = String.Format("Product/GetProducts?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
                      dates.LastExecutedDate.ToString(),
                      dates.CurrentDate.ToString(),
                      dates.TerminalId.ToString());
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    Log.WriteLog("GetProducts = Response=" + json);
                    productData = JsonConvert.DeserializeObject<ProductData>(json);
                }
                else
                {
                    Log.WriteLog("GetProducts = Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("GetProducts Exception = " + ex.ToString());
            }


            return productData;

        }
       

    }


}
