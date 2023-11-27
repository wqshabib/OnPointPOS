using Microsoft.SqlServer.Management.Smo;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using POSSUM.ApiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.StandAloneApi
{
    public class MqttHelper
    {
        public IMqttClient mqttClient;
        public List<SwishOrderApi> swishOrders = new List<SwishOrderApi>();

        public MqttHelper()
        {
            InitMqttClient();
        }

        public void InitMqttClient()
        {
            try
            {
                var serverIP = ConfigurationManager.AppSettings["MQTTSERVERIP"];
                var serverPort = Int32.Parse(ConfigurationManager.AppSettings["MQTTSERVERPORT"]);

                var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                                                    .WithTcpServer(serverIP, serverPort)
                                                    .WithCleanSession(false)
                                                    .Build();

                mqttClient = new MqttFactory().CreateMqttClient();
                mqttClient.Connected += mqttClientConnected;
                mqttClient.ApplicationMessageReceived += mqttClientApplicationMessageReceived;
                mqttClient.Disconnected += mqttClientDisconnected;
                mqttClient.ConnectAsync(mqttClientOptionsBuilder);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void mqttClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            WebApiApplication.isConnected = true;
            postPendingOrders();
        }

        private void mqttClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            WebApiApplication.isConnected = false;
            if (!mqttClient.IsConnected)
                Task.Delay(10000).ContinueWith(t => InitMqttClient());
        }

        private void mqttClientApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            //Write Code for Message Received
        }

        public bool publishMessageToTopic(string data, string topic)
        {
            try
            {
                LogWriter.LogWrite("MqttHelper: publishMessageToTopic: Publishing Message: Message: " + data + ", Topic: " + topic);

                byte[] bytesData = Encoding.UTF8.GetBytes(data);

                var messages = new MqttApplicationMessageBuilder()
                                    .WithTopic(topic)
                                    .WithPayload(bytesData)
                                    .WithExactlyOnceQoS()
                                    .WithRetainFlag()
                                    .WithRetainFlag(false)
                                    .Build();

                mqttClient.PublishAsync(messages);

                LogWriter.LogWrite("MqttHelper: publishMessageToTopic: Message Sent: Message: " + data + ", Topic: " + topic);

                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }

        public void postPendingOrders()
        {
            if (swishOrders.Count > 0)
            {
                List<SwishOrderApi> ordersToRemove = new List<SwishOrderApi>();
                foreach (var swishOrder in swishOrders)
                {
                    if (WebApiApplication.isConnected)
                    {
                        string json = JsonConvert.SerializeObject(swishOrder);
                        string topic = ConfigurationManager.AppSettings["ORDERTOPIC"] + swishOrder.Order.Id;

                        bool isSent = publishMessageToTopic(json, topic);

                        if (isSent)
                            ordersToRemove.Add(swishOrder);
                    }
                }

                if (ordersToRemove.Count > 0)
                    swishOrders.RemoveAll(obj => ordersToRemove.Contains(obj));
            }
        }

        public void setPendingOrders(SwishOrderApi swishOrder)
        {
            swishOrders.Add(swishOrder);
        }
    }
}
