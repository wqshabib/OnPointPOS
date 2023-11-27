using Bot.Messenger;
using Bot.Messenger.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace POSSUM.OrderingSystem.Controllers
{
    [RoutePrefix("api/WChat")]
    public class WChatController : ApiController
    {
        private MessengerPlatform _Bot { get; set; }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            //new LogWriter().CashGaurdLog("Initialize.. Method Calling");
            //AddLogs("POSSUM Initialize Calling...", "Initialize Calling...");
            base.Initialize(controllerContext);
            //new CustomerRepository().AddLogs("WChatController,Initialize", "controllerContext");

            /***Credentials are fetched from web.config ApplicationSettings when the CreateInstance
            ----method is called without a credentials parameter or if the parameterless constructor
            ----is used to initialize the MessengerPlatform class. This holds true for all types that inherit from
            ----Bot.Messenger.ApiBase
                _Bot = MessengerPlatform.CreateInstance();
                _Bot = new MessengerPlatform();
            ***/
            var _appSecret = ConfigurationManager.AppSettings["FBSecret"];
            var _pageToken = ConfigurationManager.AppSettings["FBToken"];
            var _verifyToken = ConfigurationManager.AppSettings["FBVerifyToken"];
            _Bot = MessengerPlatform.CreateInstance(
                    MessengerPlatform.CreateCredentials(_appSecret, _pageToken, _verifyToken));


        }

        public HttpResponseMessage Get()
        {
            //new CustomerRepository().AddLogs("WChatController,Get", "Get");
            //new LogWriter().CashGaurdLog("Get.. Method Calling...");

            var querystrings = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            // new CustomerRepository().AddLogs("WChatController,Get", "querystrings:" + querystrings);

            if (_Bot.Authenticator.VerifyToken(querystrings["hub.verify_token"]))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(querystrings["hub.challenge"], Encoding.UTF8, "text/plain")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }



        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            try
            {
                var body = await Request.Content.ReadAsStringAsync();

                LogInfo("WebHook_Received", new Dictionary<string, string>
            {
                { "Request Body", body }
            });

                //new LogWriter().CashGaurdLog("AddLogs PostAPI Calling...,Json.Decode(body) Body = " + body + ", and model = " + data);

                if (!_Bot.Authenticator.VerifySignature(Request.Headers.GetValues("X-Hub-Signature").FirstOrDefault(), body))
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                WebhookModel webhookModel = _Bot.ProcessWebhookRequest(body);
                //var data = "";// JsonConvert.SerializeObject(webhookModel);
                if (webhookModel != null)
                {
                    new LogWriter().CashGaurdLog("AddLogs PostAPI Calling..., Body = " + body);

                    if (webhookModel._Object != "page")
                        return new HttpResponseMessage(HttpStatusCode.OK);

                    foreach (var entry in webhookModel.Entries)
                    {
                        foreach (var evt in entry.Events)
                        {
                            var userProfileRsp = await _Bot.UserProfileApi.GetUserProfileAsync(evt.Sender.ID);

                            //new CustomerRepository().AddLogs("WChatController,evt.EventType", "evt.EventType: " + evt.EventType);
                            new LogWriter().CashGaurdLog("WChatController,evt.EventType ,evt.EventType: " + evt.EventType + " :body: " + body);

                            if (evt.EventType == WebhookEventType.PostbackRecievedCallback || evt.EventType == WebhookEventType.MessageReceivedCallback)
                            {
                                await _Bot.SendApi.SendActionAsync(evt.Sender.ID, SenderAction.typing_on);

                                if (evt.EventType == WebhookEventType.PostbackRecievedCallback)
                                {
                                    await ProcessPostBack(evt.Sender.ID, userProfileRsp?.FirstName, evt.Postback);
                                }
                                if (evt.EventType == WebhookEventType.MessageReceivedCallback)
                                {
                                    if (evt.Message.IsQuickReplyPostBack)
                                    {
                                        // new CustomerRepository().AddLogs("WChatController,MessageReceivedCallback", "evt.Message.IsQuickReplyPostBack " + evt.Message.IsQuickReplyPostBack);
                                        await ProcessPostBackQuickReply(evt.Sender.ID, userProfileRsp?.FirstName, evt.Message.QuickReplyPostback);
                                    }
                                    else
                                    {
                                        //new CustomerRepository().AddLogs("WChatController,MessageReceivedCallback", "else" + evt.Message.IsQuickReplyPostBack);
                                        //await WelcomeMessageToUser(evt);
                                        //await PersistentMenu(evt);
                                        await ResendMessageToUser(evt);
                                        // await ConfirmIfCorrect(evt);
                                    }
                                }
                            }
                            if (evt.EventType == WebhookEventType.ReferralCallback)
                            {
                                await _Bot.SendApi.SendActionAsync(evt.Sender.ID, SenderAction.typing_on);

                                try
                                {
                                    await CallRefrelProcessPostBack(evt.Sender.ID, evt.Referral.Ref);
                                }
                                catch (Exception e)
                                {
                                    new LogWriter().CashGaurdLog("Reffrel Exception...," + e.ToString());

                                }
                            }

                            await _Bot.SendApi.SendActionAsync(evt.Sender.ID, SenderAction.typing_off);

                        }
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            }
            catch (Exception e)
            {
                //AddLogs("AddLogs PostAPI Exception...:", "e.Message ." + e.Message + e.StackTrace + e.InnerException);
                new LogWriter().CashGaurdLog("AddLogs PostAPI Exception..., Body = " + e.ToString());

                return new HttpResponseMessage(HttpStatusCode.ExpectationFailed);

            }
        }
        private async Task ProcessPostBack(string userId, string username, Postback postback)
        {
            new LogWriter().CashGaurdLog("private async Task ProcessPostBack(string userId, string username, Postback postback)" + postback.Payload);

            if (postback.Payload == "get_started" && postback.Referral != null)
            {
                var msg = "";
                if (postback.Referral.Ref != null)
                {
                    msg = "You reserved " + postback.Referral.Ref;
                }
                await _Bot.SendApi.SendTextAsync(userId, $"Hello and welcome to My Restaurant! {msg}");
                await _Bot.SendApi.SendTextAsync(userId, $"Here you can order food and beverage and pay when you are at the restaurant!");
                var httpResponse = GetFBApi(userId, "get_started");
            }
            else if (postback.Payload == "get_started")
            {
                await _Bot.SendApi.SendTextAsync(userId, $"Hello and welcome to My Restaurant!");
                await _Bot.SendApi.SendTextAsync(userId, $"Here you can order food and beverage and pay when you are at the restaurant!");
                var httpResponse = GetFBApi(userId, "get_started");
            }
            else if (postback.Payload == "_ContactUs")
            {
                await ContactUs(userId);
            }

            else if (postback.Payload == "_Menu")
            {
                var httpResponse = GetFBApi(userId, "get_started");
            }
            else if (postback.Payload == "CallToWaiter")
            {
                await ContatCallToWaitor(userId); 
            }
            else if (postback.Payload == "_Continue")
            {
                var httpResponse = GetFBApi(userId, "get_started");
            }
            else if (postback.Payload == "Pay_my_note")
            {
                //var httpResponse = GetFBApi(userId, "Pay_my_note");
                await PayWith(userId);                
            }
            else if (postback.Payload == "PAYBILL_PAYLOAD")
            {
                //var httpResponse = GetFBApi(userId, "Pay_my_note");
                await PayWith(userId);
            }

            

        }
        private async Task ProcessPostBackQuickReply(string userId, string username, Postback postback)
        {

            if (postback.Payload == "_Share")
            {
                var httpResponse = GetFBApi(userId, "_Share");
                await ShareContinues(userId);
            }

            else if (postback.Payload == "_FindUs")
            {
                var httpResponse = await _Bot.SendApi.SendTextAsync(userId,
                    "Vallgatan 15 -- -Open map: " +
                    "https://www.google.com/maps/place/Bahria+Town,+Punjab/data=!4m2!3m1!1s0x38dfecfd4b5f068f:0x2cdc7711ff85ef15?ved=2ahUKEwjz95-snqnfAhUJUlAKHbX-C3MQ8gEwAHoECAAQAQ'",
                    new List<QuickReply>
            {
                new QuickReply
                {
                    ContentType = QuickReplyContentType.text,
                    Title = "Continue",
                    ImageUrl="https://cdn3.iconfinder.com/data/icons/street-food-and-food-trucker-1/64/pizza-fast-food-bake-bread-512.png",
                    Payload = "_Continue"
                 }
              });

            }
            else if (postback.Payload == "_Menu")
            {
                var httpResponse = GetFBApi(userId, "get_started");
            }

            else if (postback.Payload == "_Continue")
            {
                var httpResponse = GetFBApi(userId, "get_started");
            }
            else if (postback.Payload == "_NoThanks")
            {

            }
            else if (postback.Payload == "_CallWaiter")
            {
                var response = await _Bot.SendApi.SendTextAsync(userId, $"William or Anna will be coming to you right away.");

            }
        }

        private async Task ContactUs(string Sender)
        {
            SendApiResponse sendQuickReplyResponse = await _Bot.SendApi.SendTextAsync(Sender,
                    "Test---Öppettider:Måndag - Fredag 12:00 - 00:00 Lördag - Söndag 14:00 - 02:00 Telefon 111 222 333 - 444",
                new List<QuickReply>
            {
                new QuickReply
                {
                    ContentType = QuickReplyContentType.text,
                    Title = "Share",
                    Payload = "_Share"
                },
                new QuickReply
                {
                    ContentType = QuickReplyContentType.text,
                    Title = "Find us",
                    Payload = "_FindUs"
                },
                 new QuickReply
                {
                    ContentType = QuickReplyContentType.text,
                    Title = "Menu",
                    Payload = "_Menu"
                }
            });
            LogSendApiResponse(sendQuickReplyResponse);
        }

        private async Task PayWith(string userId)
        {
            new LogWriter().CashGaurdLog("PayWith PayWith PayWith method");
            await _Bot.SendApi.SendTextAsync(userId, $"Your note \n" +
                     $"--- \n" +
                     $"1 x Margherita - 10,00 kr \n" +
                     $"--- \n" +
                     $"Betalningsförmedlingsavgift  5,00 kr \n" +
                     $"--- \n" +
                     $"Totalt (SEK): 15,00 kr \n");
            var httpResponse = GetFBApi(userId, "Pay_my_note");
        }


        private async Task ContatCallToWaitor(string Sender)
        {
            SendApiResponse sendQuickReplyResponse = await _Bot.SendApi.SendTextAsync(Sender, ".", new List<QuickReply>
            {
                new QuickReply
                {
                    ContentType = QuickReplyContentType.text,
                    Title = "No Thanks",
                    ImageUrl="https://cdn3.iconfinder.com/data/icons/street-food-and-food-trucker-1/64/pizza-fast-food-bake-bread-512.png",
                    Payload = "_NoThanks"
                 },
                new QuickReply
                {
                    ContentType = QuickReplyContentType.text,
                    Title = "Yes Please",
                    ImageUrl="https://cdn3.iconfinder.com/data/icons/street-food-and-food-trucker-1/64/pizza-fast-food-bake-bread-512.png",
                    Payload = "_CallWaiter"
                 }
             });
            LogSendApiResponse(sendQuickReplyResponse);
        } 

        private async Task ShareContinues(string Sender) 
        {
            SendApiResponse sendQuickReplyResponse = await _Bot.SendApi.SendTextAsync(Sender, ".", new List<QuickReply>
            {
                new QuickReply
                {
                    ContentType = QuickReplyContentType.text,
                    Title = "Continue",
                    ImageUrl="https://cdn3.iconfinder.com/data/icons/street-food-and-food-trucker-1/64/pizza-fast-food-bake-bread-512.png",
                    Payload = "_Continue"
                 }
             });
            LogSendApiResponse(sendQuickReplyResponse);
           
        }
        private async Task CallRefrelProcessPostBack(string senderId, string refId)
        {
            try
            {
                //new CustomerRepository().AddLogs("WChatController,CallRefrelProcessPostBack", "postback.Payload: " + postback.Payload + "postback.Referral.Ref);: " + postback.Referral.Ref);
                var tableId = refId;
                var usrId = senderId;
                await _Bot.SendApi.SendTextAsync(usrId, $"Hello and welcome again now you are at table {tableId}");
                var httpResponse = GetFBApi(senderId, "get_started");
            }
            catch (Exception e)
            {
                //new CustomerRepository().AddLogs("WChatController,ReferralCallback exception", e.Message + " : " + e.InnerException);

            }

        }





        private async Task ResendMessageToUser(WebhookEvent evt)
        {

            SendApiResponse response = new SendApiResponse();

            if (evt.Message.Attachments == null)
            {
                string text = evt.Message?.Text;
                //new CustomerRepository().AddLogs("WChatController,ResendMessageToUser", "text: " + text);               
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = "Hello :)";
                    response = await _Bot.SendApi.SendTextAsync(evt.Sender.ID, $"{text}");
                    var httpResponse = GetFBApi(evt.Sender.ID, "get_started");
                }
                else if (text.ToLower().Contains("hi") || text.ToLower().Contains("hello"))
                {
                    text = "Hello :)";
                    response = await _Bot.SendApi.SendTextAsync(evt.Sender.ID, $"{text}");
                    var httpResponse = GetFBApi(evt.Sender.ID, "get_started");

                }

                else if (text.ToLower() == "Help")
                {
                    var httpResponse = GetFBApi(evt.Sender.ID, "get_started");
                    //await ConfirmIfCorrect(evt);
                }
                else if (text.ToLower() == "bye")
                {
                    text = "Thank you Sir Bye Take Care :)";
                    response = await _Bot.SendApi.SendTextAsync(evt.Sender.ID, $"{text}");
                }
                else
                {
                    var httpResponse = GetFBApi(evt.Sender.ID, "get_started");
                    //await ConfirmIfCorrect(evt);
                }
            }
            else
            {
                foreach (var attachment in evt.Message.Attachments)
                {
                    if (attachment.Type != AttachmentType.fallback && attachment.Type != AttachmentType.location)
                    {
                        response = await _Bot.SendApi.SendAttachmentAsync(evt.Sender.ID, attachment);
                    }
                }
            }

            LogSendApiResponse(response);
        }



        public async Task GetFBApi(string userId, string jsonId)
        {
            try
            {
                var pageToketen = ConfigurationManager.AppSettings["FBToken"];
                var url = "https://graph.facebook.com/v2.6/me/messages?access_token=" + pageToketen;
                //new CustomerRepository().AddLogs("WChatController,GetFBApi", "initialize+url" + url + " : " + jsonId);

                //send api call 
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                string json = GetMyJson(userId, jsonId);
                // new CustomerRepository().AddLogs("WChatController,GetFBApi", "url: " + url + " :json: " + json);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // new CustomerRepository().AddLogs("WChatController,GetFBApi", "url: " + url + " :json: " + json + " :httpResponse: " + httpResponse);

                //SendApiResponse response = new SendApiResponse();
                //response = await _Bot.SendApi.SendTextAsync(userId, $"{response}");
                //LogSendApiResponse(response);
            }
            catch (Exception e)
            {
                //new CustomerRepository().AddLogs("WChatController,Exceptions", "url: " + e.Message + " : " + e);

            }
        }
        public string GetMyJson(string userId, string jsonId)
        {
            // new CustomerRepository().AddLogs("WChatController,GetMyJson", "userId " + userId + " jsonId" + jsonId);
            new LogWriter().CashGaurdLog("call get my json jsonId");

            var json = "";
            if (jsonId == "get_started")
            {
                var filePath = string.Format("{0}/{1}", ConfigurationManager.AppSettings["_Hostpath"], "storages/JsonFiles/get_started.json");
                var path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\storages\JsonFiles\get_started.json";

                using (StreamReader r = new StreamReader(path))
                {
                    string strng = r.ReadToEnd().ToString();
                    json = strng.Replace("<PSID>", userId).Replace("'", "\"");

                }
            }
            else if (jsonId == "_Share")
            {
                var filePath = string.Format("{0}/{1}", ConfigurationManager.AppSettings["_Hostpath"], "storages/JsonFiles/Share.json");
                var path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\storages\JsonFiles\Share.json";

                using (StreamReader r = new StreamReader(path))
                {
                    string strng = r.ReadToEnd().ToString();
                    json = strng.Replace("<PSID>", userId).Replace("'", "\"");

                }
            }
            else if (jsonId == "Pay_my_note") 
            {
                new LogWriter().CashGaurdLog("jsonId == Pay_my_note");

                var filePath = string.Format("{0}/{1}", ConfigurationManager.AppSettings["_Hostpath"], "storages/JsonFiles/Pay_my_note.json");
                var path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\storages\JsonFiles\pay_with.json";
               
                using (StreamReader r = new StreamReader(path))
                {
                    string strng = r.ReadToEnd().ToString();
                    json = strng.Replace("<PSID>", userId).Replace("'", "\"");

                }
                new LogWriter().CashGaurdLog("jsonId == Pay_my_note _+ json "+ json);

            }
            else if (jsonId == "2")
            {
                var filePath = string.Format("{0}/{1}", ConfigurationManager.AppSettings["_Hostpath"], "storages/JsonFiles/list.json");
                var path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\storages\JsonFiles\list.json";
                // new CustomerRepository().AddLogs("WChatController,filePath", "filePath " + filePath);

                //var filePath= http://ir-api.mofr.se/storages/
                using (StreamReader r = new StreamReader(path))
                {
                    string strng = r.ReadToEnd().ToString();
                    json = strng.Replace("RECIPIENT_ID", userId).Replace("'", "\"");

                }
            }
            else if (jsonId == "3")
            {
                var filePath = string.Format("{0}/{1}", ConfigurationManager.AppSettings["_Hostpath"], "storages/JsonFiles/generic.json");
                var path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\storages\JsonFiles\generic.json";

                using (StreamReader r = new StreamReader(path))
                {
                    string strng = r.ReadToEnd().ToString();
                    json = strng.Replace("<PSID>", userId).Replace("'", "\"");
                }
            }
            return json;

        }



        private void WelcomeMessageToUser()
        {
            var httpResponse = GetProfileApis("1", 11);
        }
        private void PersistentMenu()
        {
            var httpRespons1 = GetProfileApis("1", 22);
        }

        public async Task GetProfileApis(string userId, int jsonId)
        {
            try
            {
                //new CustomerRepository().AddLogs("WChatController,GetProfileApis", "initialize");
                var pageToketen = ConfigurationManager.AppSettings["FBToken"];
                var url = "https://graph.facebook.com/v2.6/me/messenger_profile?access_token=" + pageToketen;
                //send api call 
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                string json = GetMyJson2(jsonId);
                // new CustomerRepository().AddLogs("WChatController,GetProfileApis", "url: " + url + " :json: " + json);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //new CustomerRepository().AddLogs("WChatController,GetProfileApis", "userId: " + userId + "url: " + url + " :json: " + json + " :httpResponse: " + httpResponse);

                //SendApiResponse response = new SendApiResponse();
                //response = await _Bot.SendApi.SendTextAsync(userId, $"{response}");
                //LogSendApiResponse(response);
            }
            catch (Exception e)
            {
                //new CustomerRepository().AddLogs("WChatController,GetFBApi", "url: " + e.Message);

            }
        }

        public string GetMyJson2(int jsonId)
        {
            var json = "";
            if (jsonId == 11)
            {
                var strng = "{'get_started': {'payload': 'get_started'}}";
                json = strng.Replace("'", "\"");
            }
            else if (jsonId == 22)
            {
                var strng = "{'persistent_menu': [{'locale': 'default','composer_input_disabled': false,'call_to_actions': [{'title': 'My Account','type': 'nested','call_to_actions': ["
                            + "{'title': 'Pay Bill','type': 'postback','payload': 'PAYBILL_PAYLOAD'},"
                            + "{'type': 'web_url','title': 'Latest News','url': 'https://www.messenger.com/','webview_height_ratio': 'full'}]}]}]}";
                json = strng.Replace("'", "\"");
            }
            return json;
        }

        private static void LogSendApiResponse(SendApiResponse response)
        {
            LogInfo("SendApi Web Request", new Dictionary<string, string>
            {
                { "Response", response?.ToString() }
            });

        }

        private static void LogInfo(string eventName, Dictionary<string, string> telemetryProperties)
        {
            //new CustomerRepository().AddLogs("WChatController,LogInfo", "eventName:" + eventName + " " + telemetryProperties.ToString());

            //Log telemetry in DB or Application Insights
        }

        private void AddDataLogs(string body)
        {

            new LogWriter().CashGaurdLog(body);

        }

        [HttpPost]
        [Route("NewApi")]
        public async Task<HttpResponseMessage> NewApi()
        {
            {

                try
                {
                    var body = await Request.Content.ReadAsStringAsync();
                    dynamic data = System.Web.Helpers.Json.Decode(body);

                    //new LogWriter().CashGaurdLog("AddLogs PostAPI Calling...,Json.Decode(body) Body = " + body + ", and model = " + data);
                    dynamic entry1 = data.entry;
                    //new LogWriter().CashGaurdLog("AddLogs PostAPI Calling...,Json.Decode(body) entry1 " + entry1.Entries);
                    WebhookModel webhookModel = _Bot.ProcessWebhookRequest(body);
                    //foreach (dynamic item in entry1.messaging)
                    //{
                    //    //new LogWriter().CashGaurdLog("WChatController,dynamic foreach (dynamic item in entry1): " + item);

                    //    foreach (dynamic evt in item.Events)
                    //    {
                    //        //new LogWriter().CashGaurdLog("WChatController,dynamic evt.EventType ,evt.EventType: " + evt.EventType);
                    //    }
                    //}
                    if (webhookModel != null)
                    {
                        new LogWriter().CashGaurdLog("AddLogs PostAPI Calling..., Body = " + body + ", and model = " + data);

                        if (webhookModel._Object != "page")
                            return new HttpResponseMessage(HttpStatusCode.OK);

                        foreach (var entry in webhookModel.Entries)
                        {
                            foreach (var evt in entry.Events)
                            {
                                var userProfileRsp = await _Bot.UserProfileApi.GetUserProfileAsync(evt.Sender.ID);

                                //new CustomerRepository().AddLogs("WChatController,evt.EventType", "evt.EventType: " + evt.EventType);
                                new LogWriter().CashGaurdLog("WChatController,evt.EventType ,evt.EventType: " + evt.EventType);

                                if (evt.EventType == WebhookEventType.PostbackRecievedCallback || evt.EventType == WebhookEventType.MessageReceivedCallback)
                                {
                                    await _Bot.SendApi.SendActionAsync(evt.Sender.ID, SenderAction.typing_on);


                                    if (evt.EventType == WebhookEventType.PostbackRecievedCallback)
                                    {
                                        //new CustomerRepository().AddLogs("WChatController,PostbackRecievedCallback", "evt.EventType: " + evt.EventType);
                                        await ProcessPostBack(evt.Sender.ID, userProfileRsp?.FirstName, evt.Postback);
                                    }
                                    if (evt.EventType == WebhookEventType.MessageReceivedCallback)
                                    {
                                        if (evt.Message.IsQuickReplyPostBack)
                                        {
                                            // new CustomerRepository().AddLogs("WChatController,MessageReceivedCallback", "evt.Message.IsQuickReplyPostBack " + evt.Message.IsQuickReplyPostBack);
                                            // await ProcessPostBack(evt.Sender.ID, userProfileRsp?.FirstName, evt.Message.QuickReplyPostback);
                                        }
                                        else
                                        {
                                            //new CustomerRepository().AddLogs("WChatController,MessageReceivedCallback", "else" + evt.Message.IsQuickReplyPostBack);
                                            //await WelcomeMessageToUser(evt);
                                            //await PersistentMenu(evt);
                                            await ResendMessageToUser(evt);
                                            // await ConfirmIfCorrect(evt);
                                        }
                                    }
                                }
                                if (evt.EventType == WebhookEventType.ReferralCallback)
                                {
                                    new LogWriter().CashGaurdLog("WChatController,in reffrel initialize condition ");

                                    await _Bot.SendApi.SendActionAsync(evt.Sender.ID, SenderAction.typing_on);

                                    try
                                    {

                                        new LogWriter().CashGaurdLog("WChatController,ReferralCallback ,else case: " + evt.Referral.Ref);

                                        await CallRefrelProcessPostBack(evt.Sender.ID, evt.Referral.Ref);

                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }

                                await _Bot.SendApi.SendActionAsync(evt.Sender.ID, SenderAction.typing_off);

                            }
                        }

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);

                }
                catch (Exception e)
                {
                    return new HttpResponseMessage(HttpStatusCode.ExpectationFailed);

                }

            }
        }
    }
}
