using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.StandAloneApi.Handlers;
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
    [System.Web.Http.RoutePrefix("mapi/Order")]
    public class OrderController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public OrderController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostOrders")]
        public HttpResponseMessage PostOrders(List<OrderApi> orders)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                string objJson = JsonConvert.SerializeObject(orders);
                LogWriter.LogWrite("OrderController: PostOrders: CallBack: Json: " + objJson);

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<OrderApi> SuccessList = new List<OrderApi>();
                    List<OrderApi> ErrorList = new List<OrderApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var order in orders)
                        {
                            try
                            {
                                var dbObject = db.OrderMaster.FirstOrDefault(obj => obj.Id == order.Id);
                                if (dbObject == null)
                                {
                                    dbObject = OrderApi.ConvertApiModelToModel(order);
                                    db.OrderMaster.Add(dbObject);
                                }
                                else
                                {
                                    dbObject = OrderApi.UpdateModel(dbObject, order);
                                    db.Entry(dbObject).State = EntityState.Modified;
                                }

                                LogWriter.LogWrite("OrderController: PostOrders: OrderId: " + order.Id);

                                db.SaveChanges();

                                SuccessList.Add(order);
                            }
                            catch (Exception ex)
                            {
                                LogWriter.LogWrite("OrderController: PostOrders: Exception: OrderId: " + order.Id);

                                responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                                ErrorList.Add(order);
                                LogWriter.LogWrite(ex);
                            }
                        }
                    }

                    responseApi.SuccessList = SuccessList;
                    responseApi.ErrorList = ErrorList;

                    string json = JsonConvert.SerializeObject(responseApi);
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

        [System.Web.Http.HttpPost]
        [Route("PostCompletedOrders")]
        public HttpResponseMessage PostCompletedOrders(OrderApi order)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                string objJson = JsonConvert.SerializeObject(order);
                LogWriter.LogWrite("OrderController: PostCompletedOrders: CallBack: Json: " + objJson);

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<OrderApi> SuccessList = new List<OrderApi>();
                    List<OrderApi> ErrorList = new List<OrderApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        try
                        {
                            var dbOrder = db.OrderMaster.FirstOrDefault(obj => obj.Id == order.Id);
                            if (dbOrder == null)
                            {
                                dbOrder = OrderApi.ConvertApiModelToModel(order);
                                db.OrderMaster.Add(dbOrder);
                            }
                            else
                            {
                                dbOrder = OrderApi.UpdateModel(dbOrder, order);
                                db.Entry(dbOrder).State = EntityState.Modified;
                            }

                            LogWriter.LogWrite("OrderController: PostCompletedOrders: OrderId: " + order.Id);

                            foreach (var orderLine in order.OrderLines)
                            {
                                var dbOrderLine = db.OrderDetail.FirstOrDefault(obj => obj.Id == orderLine.Id);
                                if (dbOrderLine == null)
                                {
                                    dbOrderLine = OrderLineApi.ConvertApiModelToModel(orderLine);
                                    dbOrderLine.IsInventoryAdjusted = AdjustProductStock(db, dbOrderLine);
                                    db.OrderDetail.Add(dbOrderLine);
                                }
                                else
                                {
                                    dbOrderLine = OrderLineApi.UpdateModel(dbOrderLine, orderLine);
                                    bool isInventoryAdjusted = dbOrderLine.IsInventoryAdjusted;
                                    if (!isInventoryAdjusted)
                                        dbOrderLine.IsInventoryAdjusted = AdjustProductStock(db, dbOrderLine);
                                    db.Entry(dbOrderLine).State = EntityState.Modified;
                                }

                                LogWriter.LogWrite("OrderController: PostCompletedOrders: OrderId: " + dbOrderLine.OrderId + ", OrderLineId: " + dbOrderLine.Id);
                            }

                            foreach (var payment in order.Payments)
                            {
                                var dbPayment = db.Payment.FirstOrDefault(obj => obj.Id == payment.Id);
                                if (dbPayment == null)
                                {
                                    dbPayment = PaymentApi.ConvertApiModelToModel(payment);
                                    db.Payment.Add(dbPayment);
                                }
                                else
                                {
                                    dbPayment = PaymentApi.UpdateModel(dbPayment, payment);
                                    db.Entry(dbPayment).State = EntityState.Modified;
                                }

                                LogWriter.LogWrite("OrderController: PostCompletedOrders: OrderId: " + dbPayment.OrderId + ", PaymentId: " + dbPayment.Id);
                            }

                            foreach (var receipt in order.Receipts)
                            {
                                var dbReceipt = db.Receipt.FirstOrDefault(obj => obj.ReceiptId == receipt.ReceiptId);
                                if (dbReceipt == null)
                                {
                                    dbReceipt = ReceiptApi.ConvertApiModelToModel(receipt);
                                    db.Receipt.Add(dbReceipt);
                                }
                                else
                                {
                                    dbReceipt = ReceiptApi.UpdateModel(dbReceipt, receipt);
                                    db.Entry(dbReceipt).State = EntityState.Modified;
                                }

                                LogWriter.LogWrite("OrderController: PostCompletedOrders: OrderId: " + dbReceipt.OrderId + ", ReceiptId: " + dbReceipt.ReceiptId);
                            }

                            db.SaveChanges();

                            LogWriter.LogWrite("OrderController: PostCompletedOrders: Done: OrderId: " + order.Id);

                            SuccessList.Add(order);
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogWrite("OrderController: PostCompletedOrders: Exception: OrderId: " + order.Id);

                            responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                            ErrorList.Add(order);
                            LogWriter.LogWrite(ex);
                        }
                    }

                    responseApi.SuccessList = SuccessList;
                    responseApi.ErrorList = ErrorList;

                    string json = JsonConvert.SerializeObject(responseApi); //, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss.FFFFFFFK" }
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    return httpResponseMessage;
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("OrderController: PostCompletedOrders: Exception: OrderId: " + order.Id);

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

        private bool AdjustProductStock(ApplicationDbContext dbContext, OrderLine orderLine)
        {
            var product = dbContext.Product.FirstOrDefault(obj => obj.Id == orderLine.ItemId);
            if (product != null)
            {
                ProductStockHistory productStockHistory = new ProductStockHistory();
                productStockHistory.Id = Guid.NewGuid();
                productStockHistory.ProductId = orderLine.ItemId;
                productStockHistory.CreatedOn = DateTime.Now;
                productStockHistory.UpdatedOn = DateTime.Now;

                if (product.Unit == ProductUnit.Piece)
                {
                    decimal productLastStock = product.StockQuantity;
                    decimal productNewStock = 0;

                    if (orderLine.Direction == 1)
                        productNewStock = product.StockQuantity - orderLine.Quantity;
                    else
                        productNewStock = product.StockQuantity + orderLine.Quantity;

                    product.StockQuantity = productNewStock;

                    productStockHistory.LastStock = productLastStock;
                    productStockHistory.NewStock = productNewStock;
                }
                else
                {
                    decimal? productLastWeight = (product.Weight != null) ? product.Weight : 0;
                    decimal? productNewWeight = 0;

                    if (orderLine.Direction == 1)
                        productNewWeight = productLastWeight - orderLine.Quantity;
                    else
                        productNewWeight = productLastWeight + orderLine.Quantity;

                    product.Weight = productNewWeight;

                    productStockHistory.LastStock = productLastWeight.Value;
                    productStockHistory.NewStock = productNewWeight.Value;
                }

                dbContext.ProductStockHistory.Add(productStockHistory);
                dbContext.Entry(product).State = EntityState.Modified;

                return true;
            }

            return false;
        }
    }
}
