using POSSUM.Data;
using POSSUM.MasterData;
using POSSUM.StandAloneApi.Handlers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using POSSUM.ApiModel;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.Entity;

namespace POSSUM.StandAloneApi.Controllers
{
    [System.Web.Http.RoutePrefix("mapi/SwishCallback")]
    public class SwishCallbackController : ApiController
    {
        private string connectionString = "";

        public SwishCallbackController()
        {
            
        }

        [System.Web.Http.HttpPost]
        [Route("CallBack")]
        public void CallBack(PaymentCallback paymentCallback)
        {
            try
            {
                string paymentCallbackJson = JsonConvert.SerializeObject(paymentCallback);
                LogWriter.LogWrite("SwishCallbackController: CallBack: Json: " + paymentCallbackJson);

                if (string.IsNullOrEmpty(paymentCallback.payeePaymentReference) || string.IsNullOrEmpty(paymentCallback.message))
                    return;

                using (MasterDbContext mdb = new MasterDbContext())
                {
                    Guid companyGuid = Guid.Parse(paymentCallback.message);

                    var company = mdb.Company.Where(obj => obj.Id == companyGuid).FirstOrDefault();
                    if (company != null)
                        connectionString = "Data Source=" + company.DBServer + ";Initial Catalog=" + company.DBName + ";UID=" + company.DBUser + ";Password=" + company.DBPassword + ";";

                    #if DEBUG
                    connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                    #endif
                }

                if (string.IsNullOrEmpty(connectionString))
                    return;

                SwishOrderApi swishOrder = new SwishOrderApi();
                swishOrder.SwishAmount = paymentCallback.amount;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    Guid orderGuid = Guid.ParseExact(paymentCallback.payeePaymentReference, "N");

                    var dbOrder = db.OrderMaster.FirstOrDefault(obj => obj.Id == orderGuid);
                    var dbSwishPayment = db.SwishPayment.FirstOrDefault(obj => obj.OrderId == orderGuid);

                    if (string.IsNullOrEmpty(paymentCallback.errorCode))
                    {
                        dbSwishPayment.SwishId = paymentCallback.id;
                        dbSwishPayment.SwishPaymentId = paymentCallback.paymentReference;
                        dbSwishPayment.SwishResponse = paymentCallbackJson;

                        switch (paymentCallback.status)
                        {
                            case "CREATED":
                                dbSwishPayment.SwishPaymentStatus = 1;
                                break;
                            case "PAID":
                                dbSwishPayment.SwishPaymentStatus = 2;
                                break;
                            case "DECLINED":
                                dbSwishPayment.SwishPaymentStatus = 3;
                                break;
                            case "ERROR":
                                dbSwishPayment.SwishPaymentStatus = 4;
                                break;
                        }

                        swishOrder.Order = OrderApi.ConvertModelToApiModel(dbOrder);
                        swishOrder.Order.SwishPayment = SwishPaymentApi.ConvertModelToApiModel(dbSwishPayment);
                        swishOrder.Status = 0;
                        swishOrder.Message = paymentCallback.status;
                    }
                    else
                    {
                        dbSwishPayment.SwishId = paymentCallback.id;
                        dbSwishPayment.SwishPaymentId = paymentCallback.paymentReference;
                        dbSwishPayment.SwishPaymentStatus = 4;
                        dbSwishPayment.SwishResponse = paymentCallbackJson;

                        swishOrder.Order = OrderApi.ConvertModelToApiModel(dbOrder);
                        swishOrder.Order.SwishPayment = SwishPaymentApi.ConvertModelToApiModel(dbSwishPayment);
                        swishOrder.Status = -1;
                        swishOrder.Message = paymentCallback.errorMessage;
                    }

                    db.Entry(dbSwishPayment).State = EntityState.Modified;

                    db.SaveChanges();
                }

                if (WebApiApplication.isConnected)
                {
                    string json = JsonConvert.SerializeObject(swishOrder);
                    string topic = ConfigurationManager.AppSettings["ORDERTOPIC"] + swishOrder.Order.Id;

                    bool isSent = WebApiApplication.mqttHelper.publishMessageToTopic(json, topic);
                    if (!isSent)
                        WebApiApplication.mqttHelper.setPendingOrders(swishOrder);
                }
                else
                {
                    WebApiApplication.mqttHelper.setPendingOrders(swishOrder);
                    WebApiApplication.mqttHelper.InitMqttClient();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
            }
        }

        [System.Web.Http.HttpPost]
        [Route("CallBackV2")]
        public void CallBackV2(PaymentCallback paymentCallback)
        {
            LogWriter.LogWrite("PrepareHttpClientAndHandler: CallBackV2: Json: " + JsonConvert.SerializeObject(paymentCallback));
        }
    }
}
