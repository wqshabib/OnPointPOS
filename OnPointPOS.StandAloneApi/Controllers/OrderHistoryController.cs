using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace POSSUM.StandAloneApi.Controllers
{
    [Authorize]
    [System.Web.Http.RoutePrefix("mapi/OrderHistory")]
    public class OrderHistoryController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public OrderHistoryController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpGet]
        [Route("GetOrdersHistory")]
        public HttpResponseMessage GetOrdersHistory([FromUri] DatesApi dates)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    DateTime FROM = dates.From.AddMinutes(-5);
                    DateTime TO = dates.To;
                    int pageNo = dates.PageNo;
                    int pageSize = dates.PageSize;

                    List<OrderHistoryApi> ordersHistory = new List<OrderHistoryApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        var dbOrders = db.OrderMaster.Where(obj => obj.CreationDate >= FROM && obj.CreationDate <= TO && obj.TerminalId == dates.TerminalId)
                                                    .OrderByDescending(obj => obj.CreationDate)
                                                    .Skip(pageNo * pageSize)
                                                    .Take(pageSize)
                                                    .ToList();

                        if (dbOrders != null && dbOrders.Count > 0)
                        {
                            foreach (var order in dbOrders)
                            {
                                OrderHistoryApi orderHistory = OrderHistoryApi.ConvertModelToApiModel(order);

                                var dbOrderLines = db.OrderDetail.Where(obj => obj.OrderId == order.Id).ToList();
                                var dbPayments = db.Payment.Where(obj => obj.OrderId == order.Id).ToList();
                                var dbReceipts = db.Receipt.Where(obj => obj.OrderId == order.Id).ToList();

                                if (dbOrderLines != null && dbOrderLines.Count > 0)
                                    orderHistory.OrderLines = dbOrderLines.Select(obj => OrderLineApi.ConvertModelToApiModel(obj)).ToList();
                                else
                                    orderHistory.OrderLines = new List<OrderLineApi>();

                                if (dbPayments != null && dbPayments.Count > 0)
                                    orderHistory.Payments = dbPayments.Select(obj => PaymentApi.ConvertModelToApiModel(obj)).ToList();
                                else
                                    orderHistory.Payments = new List<PaymentApi>();

                                if (dbReceipts != null && dbReceipts.Count > 0)
                                    orderHistory.Receipts = dbReceipts.Select(obj => ReceiptApi.ConvertModelToApiModel(obj)).ToList();
                                else
                                    orderHistory.Receipts = new List<ReceiptApi>();

                                ordersHistory.Add(orderHistory);
                            }
                        }
                    }

                    string json = JsonConvert.SerializeObject(ordersHistory);
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    return httpResponseMessage;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                    return Request.CreateResponse(HttpStatusCode.BadGateway, ex.Message);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }            
        }
    }
}
