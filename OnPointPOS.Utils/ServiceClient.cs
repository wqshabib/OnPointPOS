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
    public class ServiceClient
    {

        string _baseAddress = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
        string access_Token = "";
        DateTime LAST_EXECUTED_DATETIME = DateTime.Now;



        string TerminalId = ConfigurationManager.AppSettings["TerminalId"];

        public ServiceClient(string baseUrl, string user, string password)
        {
            if (!string.IsNullOrEmpty(baseUrl))
                _baseAddress = baseUrl;
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

        internal SettingData GetSettings(DateTime lAST_EXECUTED_DATETIME, DateTime eXECUTED_DATETIME, Guid terminalId)
        {
            Dates dates = new Dates { CurrentDate = eXECUTED_DATETIME, LastExecutedDate = lAST_EXECUTED_DATETIME, TerminalId = terminalId };
            SettingData settingData = new SettingData();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/Setting/GetSettings?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
                  dates.LastExecutedDate.ToString(),
                  dates.CurrentDate.ToString(),
                  dates.TerminalId.ToString());
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                var json = response.Content.ReadAsStringAsync().Result;
                settingData = JsonConvert.DeserializeObject<SettingData>(json);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return settingData;
        }




        internal string GetLatestFileToDownload(DateTime lAST_EXECUTED_DATETIME, DateTime eXECUTED_DATETIME, Guid terminalId)
        {
            Dates dates = new Dates { CurrentDate = eXECUTED_DATETIME, LastExecutedDate = lAST_EXECUTED_DATETIME, TerminalId = terminalId };
            var data = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/File/GetNewFile?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
                  dates.LastExecutedDate.ToString(),
                  dates.CurrentDate.ToString(),
                  dates.TerminalId.ToString());
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                data = response.Content.ReadAsStringAsync().Result;
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(data);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return data;
        }

        public bool SyncOrder(Order order)
        {
            var response = PostOrder(order);
            return response.IsSuccessStatusCode;
        }

        public HttpResponseMessage PostOrder(Order order)
        {

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
                client.BaseAddress = new Uri(_baseAddress);

                HttpResponseMessage response = client.PostAsJsonAsync<Order>("api/Order/PostOrder", order).Result;

                var data = JsonConvert.SerializeObject(order);
                if (response.IsSuccessStatusCode)
                {
                    Log.WriteLog("PostOrder..." + order.Id);
                }
                else
                {
                    Log.WriteLog("Sync Order Fail:> Error Code::: " + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
                return response;
            }

            catch (Exception ex)
            {
                Log.WriteLog(ex.Message);
                throw ex;
            }
        }

        public bool UpdateOnlineOrderStatus(string orderId, int status)
        {

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
                client.BaseAddress = new Uri(_baseAddress);
                var url = String.Format("api/Order/UpdateOrderStatus/{0}/{1}", orderId, status);
                HttpResponseMessage response = client.GetAsync(url).Result;
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HttpResponseMessage PostCashDrawerLog(CashDrawerLog model)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<CashDrawerLog>("api/CashDrawerLog/PostCashDrawerLog", model).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("PostCashDrawerLog Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;
        }

        public HttpResponseMessage PostInvoiceCounter(InvoiceCounter model)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<InvoiceCounter>("api/Setting/PostInvoiceCounter", model).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("PostInvoiceCounter Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;
        }
        public HttpResponseMessage PostDepositHistory(DepositHistory model)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<DepositHistory>("api/DepositHistory/PostDepositHistory", model).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("PostDepositHistory Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;
        }
        internal HttpResponseMessage PostCustomer(Customer customer)
        {


            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<Customer>("api/Customer/PostCustomer", customer).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Upload Customer Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;
        }

        public List<Customer> GetCustomers(DateTime LastExecutedDate, DateTime executedDate)
        {
            Log.WriteLog("getting customers ");
            Dates dates = new Dates { CurrentDate = DateTime.Now, LastExecutedDate = LastExecutedDate, TerminalId = default(Guid) };
            List<Customer> customerData = new List<Customer>();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/customer/GetCustomers?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
                  dates.LastExecutedDate.ToString(),
                  dates.CurrentDate.ToString(),
                  dates.TerminalId.ToString());
            Log.WriteLog("GetCustomers api calling url: " + url);

            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                Log.WriteLog("GetCustomers api calling response.IsSuccessStatusCode: " + response.IsSuccessStatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Log.WriteLog("GetCustomers api calling response.json: " + json);

                customerData = JsonConvert.DeserializeObject<List<Customer>>(json);
                Log.WriteLog("customerData response.json : " + JsonConvert.SerializeObject(customerData));

            }
            else
            {
                Log.WriteLog("GetCustomers api calling else case " + response.IsSuccessStatusCode + " : Message - " + response.ReasonPhrase);
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return customerData;

        }

        public List<CashDrawerLog> GetCashDrawerLog(DateTime lastExecutedDate, DateTime executedDate, Guid terminalId)
        {
            try
            {
                var lstCashDrawerLog = new List<CashDrawerLog>();
                Log.WriteLog("GetCashDrawerLog called...");
                var model = new DatesWithPagingRequestModel
                {
                    CurrentDate = DateTime.Now,
                    LastExecutedDate = lastExecutedDate,
                    TerminalId = terminalId,
                    PageNo = 0,
                    PageSize = 10
                };

                var lst = GetCashDrawerLogFromAPIs(model);

                while (lst != null && lst.Count > 0)
                {
                    lstCashDrawerLog.AddRange(lst);
                    ++model.PageNo;
                    lst = GetCashDrawerLogFromAPIs(model);
                }

                return lstCashDrawerLog;
            }
            catch (Exception ex)
            {
                Log.WriteLog("GetCashDrawerLog Exception: " + ex.ToString());
                return new List<CashDrawerLog>();
            }
        }

        public List<CashDrawerLog> GetCashDrawerLogFromAPIs(DatesWithPagingRequestModel model)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = new Uri(_baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url = String.Format("api/CashDrawerLog/" +
                    "GetCashDrawerLog?dates.LastExecutedDate={0}&" +
                    "dates.CurrentDate={1}&dates.TerminalId={2}&" +
                    "PageNo={3}&PageSize={4}",
                      model.LastExecutedDate.ToString(),
                      model.CurrentDate.ToString(),
                      model.TerminalId.ToString(),
                      model.PageNo,
                      model.PageSize);

                Log.WriteLog("GetCashDrawerLogFromAPIs URL: " + url);

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    Log.WriteLog("GetCashDrawerLogFromAPIs Success: " + response.IsSuccessStatusCode);
                    var json = response.Content.ReadAsStringAsync().Result;
                    var lst = JsonConvert.DeserializeObject<List<CashDrawerLog>>(json);
                    return lst;
                }
                else
                {
                    Log.WriteLog("GetCashDrawerLogFromAPIs Failed: IsSuccessStatusCode=" + response.IsSuccessStatusCode + " : Message=" + response.ReasonPhrase);
                    return new List<CashDrawerLog>();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("GetCashDrawerLogFromAPIs Exception: " + ex.ToString());
                return new List<CashDrawerLog>();
            }
        }

        public List<DepositHistory> GetDepositHistory(DateTime lastExecutedDate, DateTime executedDate, Guid terminalId)
        {
            try
            {
                var lstDepositHistory = new List<DepositHistory>();
                Log.WriteLog("GetDepositHistory called...");
                var model = new DatesWithPagingRequestModel
                {
                    CurrentDate = DateTime.Now,
                    LastExecutedDate = lastExecutedDate,
                    TerminalId = terminalId,
                    PageNo = 0,
                    PageSize = 10
                };

                var lst = GetDepositHistoryFromAPIs(model);

                while (lst != null && lst.Count > 0)
                {
                    lstDepositHistory.AddRange(lst);
                    ++model.PageNo;
                    lst = GetDepositHistoryFromAPIs(model);
                }

                return lstDepositHistory;
            }
            catch (Exception ex)
            {
                Log.WriteLog("GetDepositHistory Exception: " + ex.ToString());
                return new List<DepositHistory>();
            }
        }

        public List<DepositHistory> GetDepositHistoryFromAPIs(DatesWithPagingRequestModel model)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = new Uri(_baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url = String.Format("api/DepositHistory/" +
                    "GetDepositHistory?dates.LastExecutedDate={0}&" +
                    "dates.CurrentDate={1}&dates.TerminalId={2}&" +
                    "PageNo={3}&PageSize={4}",
                      model.LastExecutedDate.ToString(),
                      model.CurrentDate.ToString(),
                      model.TerminalId.ToString(),
                      model.PageNo,
                      model.PageSize);

                Log.WriteLog("GetDepositHistoryFromAPIs URL: " + url);

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    Log.WriteLog("GetDepositHistoryFromAPIs Success: " + response.IsSuccessStatusCode);
                    var json = response.Content.ReadAsStringAsync().Result;
                    var lst = JsonConvert.DeserializeObject<List<DepositHistory>>(json);
                    return lst;
                }
                else
                {
                    Log.WriteLog("GetDepositHistoryFromAPIs Failed: IsSuccessStatusCode=" + response.IsSuccessStatusCode + " : Message=" + response.ReasonPhrase);
                    return new List<DepositHistory>();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("GetDepositHistoryFromAPIs Exception: " + ex.ToString());
                return new List<DepositHistory>();
            }
        }

        public List<InvoiceCounter> GetInvoiceCounters()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = new Uri(_baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url = @"api/Setting/GetInvoiceCounters";

                Log.WriteLog("GetInvoiceCounters URL: " + url);

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    Log.WriteLog("GetInvoiceCounters Success: " + response.IsSuccessStatusCode);
                    var json = response.Content.ReadAsStringAsync().Result;
                    var lst = JsonConvert.DeserializeObject<List<InvoiceCounter>>(json);
                    return lst;
                }
                else
                {
                    Log.WriteLog("GetInvoiceCounters Failed: IsSuccessStatusCode=" + response.IsSuccessStatusCode + " : Message=" + response.ReasonPhrase);
                    return new List<InvoiceCounter>();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("GetInvoiceCounters Exception: " + ex.ToString());
                return new List<InvoiceCounter>();
            }
        }

        public List<CustomerInvoice> GetCustomerInvoice(DateTime LastExecutedDate, DateTime executedDate, Guid terminalId)
        {
            Dates dates = new Dates { CurrentDate = DateTime.Now, LastExecutedDate = LastExecutedDate, TerminalId = terminalId };
            List<CustomerInvoice> customerInvoices = new List<CustomerInvoice>();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/Order/GetCustomerInvoice?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
                  dates.LastExecutedDate.ToString(),
                  dates.CurrentDate.ToString(),
                  dates.TerminalId.ToString());
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                var json = response.Content.ReadAsStringAsync().Result;
                customerInvoices = JsonConvert.DeserializeObject<List<CustomerInvoice>>(json);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return customerInvoices;

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

            HttpResponseMessage response = client.PostAsJsonAsync<Product>("api/Product/PostProduct", product).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Sync Product Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;
        }

        internal HttpResponseMessage PostSetting(Setting setting)
        {
            try
            {
                var settingStr = JsonConvert.SerializeObject(setting);
                Log.WriteLog("Posting setting with detail = " + settingStr);
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex);
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<Setting>("api/Setting/PostSetting", setting).Result;
            if (response.IsSuccessStatusCode)
            {
                Log.WriteLog("Sync setting Success");
            }
            else
            {
                Log.WriteLog("Sync setting Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
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

                var url = String.Format("api/product/GetProducts?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
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

        public CampaignData GetCampaignsData(Guid terminalId, DateTime LastExecutedDate, DateTime executedDate)
        {
            CampaignData campaignData = new CampaignData();

            try
            {
                Log.WriteLog("Calling GetCampaignsData with _baseAddress=" + _baseAddress + ", terminal = " + terminalId + ", LastExecutedDate=" + LastExecutedDate + ", executedDate=" + executedDate + ", and access_Token=" + access_Token);
                Dates dates = new Dates { CurrentDate = DateTime.Now, LastExecutedDate = LastExecutedDate, TerminalId = terminalId };
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = new Uri(_baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:

                var url = String.Format("api/product/GetCampaignsData?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
                      dates.LastExecutedDate.ToString(),
                      dates.CurrentDate.ToString(),
                      dates.TerminalId.ToString());
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    Log.WriteLog("GetCampaignsData = Response=" + json);
                    campaignData = JsonConvert.DeserializeObject<CampaignData>(json);
                }
                else
                {
                    Log.WriteLog("GetCampaignsData = Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("GetCampaignsData Exception = " + ex.ToString());
            }

            return campaignData;
        }

        public ProductData GetProductById(int itemId)
        {

            ProductData productData = new ProductData();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/product/GetProductById?id=" + itemId);
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                var json = response.Content.ReadAsStringAsync().Result;
                productData = JsonConvert.DeserializeObject<ProductData>(json);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return productData;

        }

        public List<ItemInventory> GetItemInventoryById(Guid itemId)
        {

            List<ItemInventory> productData = new List<ItemInventory>();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/product/GetItemInventory?id=" + itemId);
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                var json = response.Content.ReadAsStringAsync().Result;
                productData = JsonConvert.DeserializeObject<List<ItemInventory>>(json);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return productData;

        }

        public bool PostUser(UserModel user)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<UserModel>("api/User/PostUser", user).Result;
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
                //  Log.WriteLog("Sync Order Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


        }
        public UserData GetUsers(DateTime lAST_EXECUTED_DATETIME, DateTime eXECUTED_DATETIME, Guid terminalId)
        {
            Dates dates = new Dates { CurrentDate = eXECUTED_DATETIME, LastExecutedDate = lAST_EXECUTED_DATETIME, TerminalId = terminalId };
            UserData userData = new UserData();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/user/Getusers?dates.LastExecutedDate={0}&dates.CurrentDate={1}&dates.TerminalId={2}",
                  dates.LastExecutedDate.ToString(),
                  dates.CurrentDate.ToString(),
                  dates.TerminalId.ToString());
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                var json = response.Content.ReadAsStringAsync().Result;
                userData = JsonConvert.DeserializeObject<UserData>(json);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return userData;
        }

        public UserData PostAndGetUser(UserModel userModel)
        {

            UserData userData = new UserData();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/user/GetNewUsers?userModel.UserName={0}&userModel.Email={1}&userModel.Password={2}&userModel.DallasKey={3}&userModel.TerminalId={4}&userModel.OutletId={5}",
                  userModel.UserName.ToString(),
                  userModel.Email.ToString(),
                  userModel.Password.ToString(),
                  userModel.DallasKey.ToString(),
                  userModel.TerminalId.ToString(),
                  userModel.OutletId.ToString());
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                var json = response.Content.ReadAsStringAsync().Result;
                userData = JsonConvert.DeserializeObject<UserData>(json);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return userData;
        }

        internal bool SyncException(ExceptionLog exceptionLog)
        {
            var response = PostException(exceptionLog);
            return response.IsSuccessStatusCode;
        }

        public HttpResponseMessage PostException(ExceptionLog exceptionLog)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<ExceptionLog>("api/Setting/PostException", exceptionLog).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Sync Exception Log Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;

        }


        internal bool SyncDeviceLog(PaymentDeviceLog deviceLog)
        {
            var response = PostDeviceLog(deviceLog);
            return response.IsSuccessStatusCode;
        }

        public HttpResponseMessage PostDeviceLog(PaymentDeviceLog deviceLog)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<PaymentDeviceLog>("api/Setting/PostDeviceLog", deviceLog).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Sync Device Log Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;

        }

        internal bool SyncEmployeeLog(EmployeeLog employeeLog)
        {
            var response = PostEmployeeLog(employeeLog);
            return response.IsSuccessStatusCode;
        }
        public HttpResponseMessage PostEmployeeLog(EmployeeLog employeeLog)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<EmployeeLog>("api/Setting/PostEmployeeLog", employeeLog).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Sync Employee Log Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;

        }
        public bool SyncZReport(TerminalStatusLog preparedReport)
        {
            var response = PostReport(preparedReport);
            return response.IsSuccessStatusCode;
        }
        public HttpResponseMessage PostReport(TerminalStatusLog report)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<TerminalStatusLog>("api/Order/PostReports", report).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Sync Report Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;

        }

        public bool SyncCustomerInvoice(CustomerInvoice invoice)
        {
            var response = PostCustomerInvoice(invoice);
            return response.IsSuccessStatusCode;
        }
        public HttpResponseMessage PostCustomerInvoice(CustomerInvoice invoice)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.BaseAddress = new Uri(_baseAddress);

            HttpResponseMessage response = client.PostAsJsonAsync<CustomerInvoice>("api/Order/PostCustomerInvoice", invoice).Result;
            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                Log.WriteLog("Sync CustomerInvoice Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return response;

        }

        public async Task<HttpResponseMessage> Update(object list, string method)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);

            var formatter = new JsonMediaTypeFormatter();

            formatter.SerializerSettings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
            formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

            formatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;

            formatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

            var t = list.GetType();

            var content = new ObjectContent(list.GetType(), list, formatter);

            client.BaseAddress = new Uri(_baseAddress);

            var response = await client.PutAsync("/api/Order/" + method + "/update", content);

            return response;

        }

        public Task<HttpResponseMessage> Delete<T>(string id, string controller)
        {

            var client = new HttpClient();

            MediaTypeFormatter formatter = new JsonMediaTypeFormatter();

            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"]);

            var response = client.DeleteAsync(string.Format("/api/{0}/delete/{1}", controller, id));


            return response;

        }



    }
    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class Dates
    {
        public DateTime LastExecutedDate { get; set; }
        public DateTime CurrentDate { get; set; }

        public Guid TerminalId { get; set; }

    }

    public class DatesWithPagingRequestModel : Dates
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }
}
