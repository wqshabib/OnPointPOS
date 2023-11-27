using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using POSSUM.Model;
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
    [System.Web.Http.RoutePrefix("mapi/OrderDetail")]
    public class OrderDetailController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public OrderDetailController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostOrderDetails")]
        public HttpResponseMessage PostOrderDetails(List<OrderLineApi> ordersLines)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    ResponseApi responseApi = new ResponseApi();
                    List<OrderLineApi> SuccessList = new List<OrderLineApi>();
                    List<OrderLineApi> ErrorList = new List<OrderLineApi>();

                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        foreach (var ordersLine in ordersLines)
                        {
                            try
                            {
                                var dbObject = db.OrderDetail.FirstOrDefault(obj => obj.Id == ordersLine.Id);
                                if (dbObject == null)
                                {
                                    dbObject = OrderLineApi.ConvertApiModelToModel(ordersLine);
                                    dbObject.IsInventoryAdjusted = AdjustProductStock(db, dbObject);
                                    db.OrderDetail.Add(dbObject);
                                }
                                else
                                {
                                    dbObject = OrderLineApi.UpdateModel(dbObject, ordersLine);
                                    bool isInventoryAdjusted = dbObject.IsInventoryAdjusted;
                                    if (!isInventoryAdjusted)
                                        dbObject.IsInventoryAdjusted = AdjustProductStock(db, dbObject);
                                    db.Entry(dbObject).State = EntityState.Modified;
                                }

                                db.SaveChanges();

                                SuccessList.Add(ordersLine);
                            }
                            catch(Exception ex)
                            {
                                responseApi.Message = responseApi.Message == null ? ex.ToString() : responseApi.Message + ex.ToString();
                                ErrorList.Add(ordersLine);
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
