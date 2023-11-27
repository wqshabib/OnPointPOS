using POSSUM.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace POSSUM.Utils
{

    public class ExternalOrderServiceClient
    {
        string _baseAddress = ConfigurationManager.AppSettings["ExternalOrderAPI"];
        string access_Token = "";
        HttpClient client;

        public ExternalOrderServiceClient(string userName, string password)
        {
            client = new HttpClient();
            Request_Session_Token(userName, password);
        }
        private void Request_Session_Token(string userName, string password)
        {
            try
            {


                var loginModel = new LoginModel
                {
                    UserName = userName,
                    Password = password,
                    RememberMe = true
                };
                client.BaseAddress = new Uri(_baseAddress);

                HttpResponseMessage loginresponse = client.PostAsJsonAsync<LoginModel>("api/Account/Login", loginModel).Result;
                if (loginresponse.IsSuccessStatusCode)
                {
                    var json = loginresponse.Content.ReadAsStringAsync().Result;
                    //  var token = JsonConvert.DeserializeObject<Token>(json);
                    var token = Regex.Replace(json, "[@,\\.\";'\\\\]", string.Empty);
                    access_Token = token;
                }
                else
                {
                    // Log.WriteLog("Sync Product Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {


            }

            // }
        }

        public List<PendingOrderModel> GetPendingOrdersAsync()
        {


            List<PendingOrderModel> orders = new List<PendingOrderModel>();
            client.DefaultRequestHeaders.Accept.Clear();
            //  client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/ShopPrinter/GetPendingOrders?token={0}", access_Token);
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                try
                {

                    var json = response.Content.ReadAsStringAsync().Result;
                    orders = JsonConvert.DeserializeObject<List<PendingOrderModel>>(json);
                }
                catch
                {
                }
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return orders;
        }

        public PendingOrderModel GetPendingOrderDetail(string orderGuid)
        {


            PendingOrderModel orderDetail = new PendingOrderModel();
            client.DefaultRequestHeaders.Accept.Clear();
            //  client.BaseAddress = new Uri(_baseAddress);
            //  client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // New code:

            var url = String.Format("api/ShopPrinter/GetOrderDetail?token={0}&OrderGuid={1}", access_Token, orderGuid);
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {

                var json = response.Content.ReadAsStringAsync().Result;
                try
                {


                    orderDetail = JsonConvert.DeserializeObject<PendingOrderModel>(json);
                    // orderDetail = orderDetails.First();
                }
                catch (Exception ex)
                {


                }
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }


            return orderDetail;
        }

        public string PostOrderStatus(string orderGUId, int status, DateTime pickupTime, DateTime deliveryTime)
        {
            var result = "";

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var model = new OrderStatusModel
            {
                token = access_Token,
                DeliveryTime = deliveryTime,
                OrderGuid = orderGUId,
                OrderStatus = status,
                PickUpTime = pickupTime


            };
            HttpResponseMessage response = client.PostAsJsonAsync<OrderStatusModel>("api/Drivify/UpdateOrderStatus", model).Result;


            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                result = "Error:" + response.StatusCode + " : Message - " + response.ReasonPhrase;

            }

            return result;

        }


    }

    public class LUEOrderServiceClient
    {
        string _baseAddress = ConfigurationManager.AppSettings["LUEOrderAPI"];
        string access_Token = "";
        HttpClient client;

        public LUEOrderServiceClient(string url, string userName, string password)
        {
            client = new HttpClient();
            Request_Session_Token(url, userName, password);
        }
        private void Request_Session_Token(string url, string userName, string password)
        {
            try
            {


                var loginModel = new LUELoginModel
                {
                    username = userName,
                    password = password
                };
                client.BaseAddress = new Uri(_baseAddress);

                HttpResponseMessage loginresponse = client.PostAsJsonAsync<LUELoginModel>(url, loginModel).Result;
                if (loginresponse.IsSuccessStatusCode)
                {
                    var json = loginresponse.Content.ReadAsStringAsync().Result;
                    //var token = Regex.Replace(json, "[@,\\.\";'\\\\]", string.Empty);
                    var LUEToken = JsonConvert.DeserializeObject<LUEToken>(json);
                    //var token = Regex.Replace(json, "[@,\\.\";'\\\\]", string.Empty);
                    access_Token = LUEToken.token;// token.Split(':')[1].Replace("}","");
                    client.DefaultRequestHeaders.Accept.Clear();

                    //  client.BaseAddress = new Uri(_baseAddress);
                    // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", access_Token);
                    // client.DefaultRequestHeaders.Add("Authorization", access_Token);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", access_Token);
                }
                else
                {
                    // Log.WriteLog("Sync Product Fail:> Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.Message);

            }

            // }
        }


        public LUEOrder GetOrderDetail(string url)
        {
            LUEOrder orderDetail = new LUEOrder();
            try
            {


                // url = _baseAddress + url;

                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", access_Token);
                
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {

                    var json = response.Content.ReadAsStringAsync().Result;
                    try
                    {


                        orderDetail = JsonConvert.DeserializeObject<LUEOrder>(json);
                        // orderDetail = orderDetails.First();
                    }
                    catch (Exception ex)
                    {

                        Log.WriteLog(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
               Log.WriteLog(ex.Message);
            }

            return orderDetail;
        }

        public bool PostOrderStatus(string url, DateTime pickupTime, DateTime deliveryTime)
        {
            try
            {


                var result = "";

                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", access_Token);
                // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var pickupTimeJava = JavaDateTimetoTimeStamp(pickupTime);
                var deliveryTimeJava = JavaDateTimetoTimeStamp(deliveryTime);
                var pairs = new List<KeyValuePair<string, string>>
                        {

                            new KeyValuePair<string, string>( "order_ready_time", pickupTimeJava.ToString() ),
                            new KeyValuePair<string, string> ( "order_delivery_time", deliveryTimeJava.ToString() )
                        };
                var content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage response = client.PutAsync(url, content).Result;


                if (response.IsSuccessStatusCode)
                {
                    //result = response.Content.ReadAsStringAsync().Result;
                    return true;
                }
                else
                {
                    result = "Error:" + response.StatusCode + " : Message - " + response.ReasonPhrase;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.Message);
                return false;
            }
            //  return result;

        }

        public bool PostOrderCancel(string url)
        {
            var result = "";

            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", access_Token);


            var pickupTimeJava = JavaDateTimetoTimeStamp(DateTime.Now);
            var deliveryTimeJava = JavaDateTimetoTimeStamp(DateTime.Now);
            var pairs = new List<KeyValuePair<string, string>>
                        {

                            new KeyValuePair<string, string>( "order_ready_time",pickupTimeJava.ToString() ),
                            new KeyValuePair<string, string> ( "order_delivery_time",deliveryTimeJava.ToString() )
                        };
            var content = new FormUrlEncodedContent(pairs);
            HttpResponseMessage response = client.PutAsync(url, content).Result;


            if (response.IsSuccessStatusCode)
            {
                //result = response.Content.ReadAsStringAsync().Result;
                return true;
            }
            else
            {
                result = "Error:" + response.StatusCode + " : Message - " + response.ReasonPhrase;
                return false;
            }

            //  return result;

        }
        public  long JavaDateTimetoTimeStamp(DateTime dt2)
        {

            long dt2long = dt2.Ticks;
            DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0);
            long epochlong = epochTime.Ticks;
            long timeStamp = (dt2long - epochlong) / 10000;
            return timeStamp;
        }
        public DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            //ForJavaDateStampToDate
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;

            //// Unix timestamp is seconds past epoch
            //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            //dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            //return dtDateTime;
        }

    }
    public class LUELoginModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class OrderStatusModel
    {
        public string token { get; set; }
        public string OrderGuid { get; set; }
        public int OrderStatus { get; set; }
        public DateTime PickUpTime { get; set; }
        public DateTime DeliveryTime { get; set; }
    }

    /// <summary>
    /// LUE Order Model
    /// </summary>
    public class LUEOrder
    {
        public int id { get; set; }
        public decimal total_price { get; set; }
        public decimal delivery_fee { get; set; }
        public string createdAt { get; set; }
        public List<LUEItem> items { get; set; }
        public LUEstatus status { get; set; }
        public LUEDelivery delivery { get; set; }
        // [NonSerialized]
        public DateTime CreatedOn
        {
            get
            {
                double unixTimeStamp = 0;
                double.TryParse(createdAt, out unixTimeStamp);
                if (unixTimeStamp == 0)
                    return DateTime.Now;
                try
                {
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
                    return dtDateTime;
                    //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                    //return dtDateTime;
                }
                catch (Exception)
                {

                    return DateTime.Now;
                }
              
            }
        }
        public string Customer
        {
            get
            {
                return delivery != null ? delivery.name : "";
            }
        }
        public string OrderComments
        {
            get
            {
                return delivery != null ? delivery.message : "";
            }
        }
    }
    public class LUEItem
    {
        public string name { get; set; }
        public decimal price_per_item { get; set; }
        public decimal total_price { get; set; }
        public decimal amount { get; set; }
        public string[] methods { get; set; }
    }
    public class LUEstatus
    {
        public string status { get; set; }
        public string order_ready_time { get; set; }
        public string order_delivery_time { get; set; }
        public string time { get; set; }

        public DateTime Time
        {
            get
            {
                double unixTimeStamp = 0;
                double.TryParse(time, out unixTimeStamp);
                if (unixTimeStamp == 0)
                    return DateTime.Now;
                try
                {
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
                    return dtDateTime;
                    //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                    //return dtDateTime;
                }
                catch (Exception)
                {

                    return DateTime.Now;
                }

            }
        }

        public DateTime PickUpTime
        {
            get
            {
                double unixTimeStamp = 0;
                double.TryParse(order_ready_time, out unixTimeStamp);
                if (unixTimeStamp == 0)
                    return DateTime.Now;
                try
                {
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
                    return dtDateTime;
                    //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                    //return dtDateTime;
                }
                catch (Exception)
                {

                    return DateTime.Now;
                }

            }
        }

        public DateTime DeliveryTime
        {
            get
            {
                double unixTimeStamp = 0;
                double.TryParse(order_delivery_time, out unixTimeStamp);
                if (unixTimeStamp == 0)
                    return DateTime.Now;
                try
                {
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
                    return dtDateTime;
                    //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                    //return dtDateTime;
                }
                catch (Exception)
                {

                    return DateTime.Now;
                }

            }
        }
    }
    public class LUEDelivery
    {
        public string address { get; set; }
        public string deliveryMethod { get; set; }
        public string deliveryRequestedOn { get; set; }
        public string message { get; set; }
        public string name { get; set; }
        public string phonenumber { get; set; }
        public string postcode { get; set; }
    }

    public class LUEToken
    {
        public string token { get; set; }
    }

}
