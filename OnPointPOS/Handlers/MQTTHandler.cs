using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Notifications.Wpf;
using POSSUM.Data;
using POSSUM.Utility;
using POSSUM.Utils;
using POSSUM.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Handlers
{
    public static class MQTTHandler
    {

        private static bool IsTeminalClose = false;

        #region MQTT_Pos_Mini_Client_Alive

        private static IMqttClient mqttPosMiniclientAlive;

        public static void InitializeMQTTPOSSUMForMobilePosMiniClientAlive()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobilePosMiniClientAlive:");

                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttPosMiniclientAlive = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttPosMiniclientAlive.Connected += MqttPosMiniclientAlive_Connected; ;
                mqttPosMiniclientAlive.ApplicationMessageReceived += MqttPosMiniclientAlive_ApplicationMessageReceived;
                mqttPosMiniclientAlive.Disconnected += MqttPosMiniclientAlive_Disconnected;
                mqttPosMiniclientAlive.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private static void MqttPosMiniclientAlive_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            var topic = Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/CLIENTALIVE";
            var lstTopics = new List<TopicFilter>();
            lstTopics.Add(new TopicFilter(topic, MqttQualityOfServiceLevel.AtLeastOnce));
            mqttPosMiniclientAlive.SubscribeAsync(lstTopics);
            Log.WriteLog("Posmini client alive topic: " + topic);

        }


        private static void MqttPosMiniclientAlive_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttPosMiniclientAlive.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMForMobilePosMiniClientAlive());
        }

        private static void MqttPosMiniclientAlive_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var message = e.ApplicationMessage.Payload;
            string result = System.Text.Encoding.UTF8.GetString(message);
            LogWriter.LogWrite("MqttPosMiniclientAlive_ApplicationMessageReceived " + result);

            var order = JsonConvert.DeserializeObject<PossumAlive>(result);
            var msg = order.Message.Contains("Mobile Verification");
            if (msg)
            {
                var obj = new PossumAlive
                {
                    Message = "Possum is Alive",
                    IsAlive = true,
                    Id = Guid.NewGuid(),
                    Terminal = Defaults.TerminalId,
                    OutletId = Defaults.Outlet.Id.ToString()
                };
                string orderid = JsonConvert.SerializeObject(obj);
                byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                var messages = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/CLIENTALIVE").WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag(false).Build();
                mqttPosMiniclientAlive.PublishAsync(messages);
            }

        }



        public static IMqttClient mqttPoOnlineOrderClientAlive;

        public static void InitializeMQTTPOSSUMForMobilePosOnlineOrderClientAlive()
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobileClientmqttclientAlive:");

                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttPoOnlineOrderClientAlive = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttPoOnlineOrderClientAlive.Connected += MqttPoOnlineOrderClientAlive_Connected;
                mqttPoOnlineOrderClientAlive.ApplicationMessageReceived += MqttPoOnlineOrderClientAlive_ApplicationMessageReceived;
                mqttPoOnlineOrderClientAlive.Disconnected += MqttPoOnlineOrderClientAlive_Disconnected;
                mqttPoOnlineOrderClientAlive.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private static void MqttPoOnlineOrderClientAlive_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (!mqttPoOnlineOrderClientAlive.IsConnected && !IsTeminalClose)
                Task.Delay(10000).ContinueWith(t => InitializeMQTTPOSSUMForMobilePosOnlineOrderClientAlive());
        }

        private static void MqttPoOnlineOrderClientAlive_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {

            var message = e.ApplicationMessage.Payload;
            string result = System.Text.Encoding.UTF8.GetString(message);
            LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);

            var order = JsonConvert.DeserializeObject<PossumAlive>(result);
            var msg = order.Message.Contains("Mobile Verification");
            if (msg)
            {
                var obj = new PossumAlive
                {
                    Message = "Possum is Alive",
                    IsAlive = true,
                    Id = Guid.NewGuid(),
                    Terminal = Defaults.TerminalId,
                    OutletId = Defaults.Outlet.Id.ToString()
                };
                string orderid = JsonConvert.SerializeObject(obj);
                byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                var messages = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/ONLINEORDER/ISALIVE").WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag(false).Build();
                mqttPoOnlineOrderClientAlive.PublishAsync(messages);
            }
        }

        private static void MqttPoOnlineOrderClientAlive_Connected(object sender, MqttClientConnectedEventArgs e)
        {

            var topic = Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/ONLINEORDER/ISALIVE";
            var lstTopics = new List<TopicFilter>();
            lstTopics.Add(new TopicFilter(topic, MqttQualityOfServiceLevel.AtLeastOnce));
            mqttPoOnlineOrderClientAlive.SubscribeAsync(lstTopics);
            Log.WriteLog("Pos online order alive connected with topic:" + topic);

        }



        #endregion



        #region MQTT Pos Mini Orders status for Accept/Reject Info Window
        //send message back to mobile for order accepted or rejected POSMINI

        private static IMqttClient mqttclientOrderStatus;
        private static string MobileOrderId = "";
        private static bool orderStatus = false;
        public static void InitializeMQTTPOSSUMForMobileClientPosMiniOrderStatus(string orderId, bool status)
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobileClientPosMiniOrderStatus:");
                MobileOrderId = orderId;
                orderStatus = status;
                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttclientOrderStatus = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttclientOrderStatus.Connected += MqttclientOrderStatus_Connected; ;
                mqttclientOrderStatus.ApplicationMessageReceived += MqttclientOrderStatus_ApplicationMessageReceived; ;
                mqttclientOrderStatus.Disconnected += MqttclientOrderStatus_Disconnected; ;
                mqttclientOrderStatus.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        private static void MqttclientOrderStatus_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            try
            {
                var lstTopics = new List<TopicFilter>();
                lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ACCEPTED_ORDER/" + MobileOrderId, MqttQualityOfServiceLevel.AtLeastOnce));
                mqttclientOrderStatus.SubscribeAsync(lstTopics);
                //LogWriter.LogWrite("MQTTClient of Possum Mobile Clietnt Connected");
                POSSUM.ViewModels.Order orderobject = new POSSUM.ViewModels.Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = MobileOrderId,
                    Amount = 0,
                    Status = orderStatus,
                };

                if (!string.IsNullOrEmpty(MobileOrderId))
                {
                    string orderid = JsonConvert.SerializeObject(orderobject);
                    byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                    var message = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/POSMINI/ACCEPTED_ORDER/" + MobileOrderId).WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag().Build();
                    mqttclientOrderStatus.PublishAsync(message);
                }


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

            }
        }


        private static void MqttclientOrderStatus_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            LogWriter.LogWrite("Mobile Client_disconnected");
        }

        private static void MqttclientOrderStatus_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var message = e.ApplicationMessage.Payload;
            string result = System.Text.Encoding.UTF8.GetString(message);
            LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);
        }



        #endregion




        #region MQTT Pos Online Orders status for Accept/Reject Info Window
        //send message back to mobile for order accepted or rejected POSMINI

        private static IMqttClient mqttclientOnlineOrderStatus;
        private static string OnlineMobileOrderId = "";
        private static bool OnlineOrderStatus = false;
        public static void InitializeMQTTPOSSUMForMobileClientOnlineOrderStatus(string orderId, bool status)
        {
            try
            {
                Log.WriteLog("InitializeMQTTPOSSUMForMobileClientOnlineOrderStatus:");
                MobileOrderId = orderId;
                orderStatus = status;
                //LogWriter.LogWrite("InitializeMQTTPOSSUMClient.......");
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var factory = new MqttFactory();
                mqttclientOnlineOrderStatus = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder().WithTcpServer(serverIP, 8883).WithCleanSession(false).Build();
                mqttclientOnlineOrderStatus.Connected += MqttclientOnlineOrderStatus_Connected; ; ;
                mqttclientOnlineOrderStatus.ApplicationMessageReceived += MqttclientOnlineOrderStatus_ApplicationMessageReceived;
                mqttclientOnlineOrderStatus.Disconnected += MqttclientOnlineOrderStatus_Disconnected;
                mqttclientOnlineOrderStatus.ConnectAsync(options);

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private static void MqttclientOnlineOrderStatus_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            LogWriter.LogWrite("MqttclientOnlineOrderStatus_Disconnected...");
        }

        private static void MqttclientOnlineOrderStatus_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var message = e.ApplicationMessage.Payload;
            string result = System.Text.Encoding.UTF8.GetString(message);
            LogWriter.LogWrite("Client_ApplicationMessageReceived " + result);
        }

        private static void MqttclientOnlineOrderStatus_Connected(object sender, MqttClientConnectedEventArgs e)
        {

            try
            {
                var lstTopics = new List<TopicFilter>();
                lstTopics.Add(new TopicFilter(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/ONLINEORDER/ACCEPTED_ORDER/" + MobileOrderId, MqttQualityOfServiceLevel.AtLeastOnce));
                mqttclientOnlineOrderStatus.SubscribeAsync(lstTopics);
                //LogWriter.LogWrite("MQTTClient of Possum Mobile Clietnt Connected");
                POSSUM.ViewModels.Order orderobject = new POSSUM.ViewModels.Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = MobileOrderId,
                    Amount = 0,
                    Status = orderStatus,
                };

                if (!string.IsNullOrEmpty(MobileOrderId))
                {
                    string orderid = JsonConvert.SerializeObject(orderobject);
                    byte[] datainbyte = Encoding.UTF8.GetBytes(orderid);
                    var message = new MqttApplicationMessageBuilder().WithTopic(Defaults.MQTTTopic + Defaults.TerminalId.ToLower() + "/ONLINEORDER/ACCEPTED_ORDER/" + MobileOrderId).WithPayload(datainbyte).WithExactlyOnceQoS().WithRetainFlag().Build();
                    mqttclientOnlineOrderStatus.PublishAsync(message);
                }


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

            }
        }






        #endregion







        public static void SavePauseOrder(POSSUM.Model.Order order)
        {
            try
            {
                order.UserId = Defaults.User.Id;
                var res = new OrderRepository(PosState.GetInstance().Context).SaveMqttOrderMaster(order);
                if (res != null)
                {
                    new OrderRepository(PosState.GetInstance().Context).SaveOrderLines(order.OrderLines.ToList(), order);
                    new OrderRepository(PosState.GetInstance().Context).SaveUserOrder(order);
                    if (order.CustomerId != Guid.Empty && order.Customer != null)
                        new CustomerRepository(PosState.GetInstance().Context).SaveNewCustomer(order.CustomerId, order.Customer);
                }
            }
            catch (Exception e)
            {
                Log.LogException(e);
            }
        }










        public static List<CartItem> GetCartItems(POSSUM.ViewModels.Order Order)
        {

            List<CartItem> cartitems = new List<CartItem>();
            foreach (var items in Order.CartItems)
            {
                var newCrtitem = new CartItem
                {
                    CartId = items.CartId,
                    Price = Convert.ToDecimal(items.Price),
                    Title = items.Title,
                    TaxPercent = items.TaxPercent,
                    Quantity = Convert.ToInt16(items.Quantity),
                    IngredientMode = items.IngredientMode,
                    CartSubItems = GetCartSubItems(items),
                };
                cartitems.Add(newCrtitem);

            }
            return cartitems;
        }

        public static List<CartSubItem> GetCartSubItems(POSSUM.ViewModels.CartItem items)
        {

            List<CartSubItem> cartitems = new List<CartSubItem>();
            foreach (var item in items.CartSubItems)
            {
                var newCrtitem = new CartSubItem
                {
                    Id = item.Id,
                    Price = Convert.ToDecimal(item.Price),
                    Title = item.Text,
                    TaxPercent = item.TaxPercent,
                    IngredientMode = item.IngredientMode,
                    Quantity = Convert.ToInt16(items.Quantity),
                };
                cartitems.Add(newCrtitem);

            }
            return cartitems;
        }



        public static void DisconnectMQTTClientPosMini()
        {
            try
            {

                IsTeminalClose = true;
                mqttPosMiniclientAlive.DisconnectAsync();
            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }
        public static void DisconnectMQTTClientPosOnlineOrder()
        {
            try
            {

                IsTeminalClose = true;
                mqttPoOnlineOrderClientAlive.DisconnectAsync();
            }
            catch (Exception ex)
            {
                ////LogWriter.LogWrite(ex);

            }
        }





    }
}
