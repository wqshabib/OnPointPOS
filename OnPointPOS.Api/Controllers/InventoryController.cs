using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Text;
using POSSUM.Model;
using System.Data.Entity;
using POSSUM.Data;
using System.Net.Http;
using System.Collections.Generic;
using POSSUM.Api.Models;
using Newtonsoft.Json;

namespace POSSUM.Api.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    //[Authorize]
    [System.Web.Http.RoutePrefix("api/Inventory")]
    public class InventoryController : BaseAPIController
    {
        private string connectionString = "";
        private bool nonAhenticated = true;

        public InventoryController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        [HttpPost]
        [Route("PostInventory")]
        public HttpResponseMessage PostInventory(InventoryData model)
        {

            try
            {
                #if DEBUG
                connectionString = "Data Source=DESKTOP-FFLGUA4; Initial Catalog=demoretailtestuser20230106; Integrated Security=SSPI; persist security info=True;";
                #endif

                List<StockHistoryGroup> stockHistoryGroups = new List<StockHistoryGroup>();
                List<ProductStockHistory> productStockHistories = new List<ProductStockHistory>();
                List<TotalInventory> products = new List<TotalInventory>();

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    InventoryHistory inventoryHistory = new InventoryHistory();
                    inventoryHistory.InventoryHistoryId = Guid.NewGuid();
                    inventoryHistory.InventoryName = model.InventoryName;
                    inventoryHistory.CreatedDate = DateTime.Now;

                    foreach (var inventory in model.Inventory)
                    {
                        var productStocks = inventory.ProductStockHistory;

                        StockHistoryGroup stockHistoryGroup = new StockHistoryGroup();
                        stockHistoryGroup.StockHistoryGroupId = Guid.NewGuid();
                        stockHistoryGroup.GroupName = inventory.Section;
                        stockHistoryGroup.CreatedDate = DateTime.Now;
                        stockHistoryGroup.InventoryHistoryId = inventoryHistory.InventoryHistoryId;

                        stockHistoryGroups.Add(stockHistoryGroup);
                        //db.StockHistoryGroup.Add(stockHistoryGroup);

                        if (productStocks != null && productStocks.Count > 0)
                        {
                            foreach (var stockHistory in productStocks)
                            {
                                ProductStockHistory productStockHistory = new ProductStockHistory();
                                productStockHistory.Id = stockHistory.Id;
                                productStockHistory.ProductId = stockHistory.ProductId;
                                productStockHistory.StockHistoryGroupId = stockHistoryGroup.StockHistoryGroupId;
                                productStockHistory.CreatedOn = stockHistory.CreatedOn;
                                productStockHistory.UpdatedOn = stockHistory.CreatedOn;

                                var _product = db.Product.FirstOrDefault(u => u.Id == stockHistory.ProductId);
                                var productFinalStock = products.FirstOrDefault(obj => obj.ProductId == stockHistory.ProductId);

                                decimal newStock = stockHistory.NewStock;

                                if (_product.Unit == ProductUnit.Piece)
                                {
                                    stockHistory.ProductStock = _product.StockQuantity;
                                    stockHistory.LastStock = _product.StockQuantity;
                                    if (productFinalStock != null)
                                        stockHistory.LastStock = productFinalStock.TotalStock;
                                    stockHistory.NewStock = stockHistory.LastStock + newStock;
                                    stockHistory.StockValue = newStock;

                                    /*if (productFinalStock != null)
                                        stockHistory.NewStock = productFinalStock.TotalStock + newStock;
                                    else
                                        stockHistory.NewStock = stockHistory.LastStock + newStock;*/
                                }
                                else
                                {
                                    stockHistory.ProductStock = (_product.Weight != null) ? _product.Weight.Value : 0;
                                    stockHistory.LastStock = (_product.Weight != null) ? _product.Weight.Value : 0;
                                    if (productFinalStock != null)
                                        stockHistory.LastStock = productFinalStock.TotalStock;
                                    stockHistory.NewStock = stockHistory.LastStock + newStock;
                                    stockHistory.StockValue = newStock;

                                    /*if (productFinalStock != null)
                                        stockHistory.NewStock = productFinalStock.TotalStock + newStock;
                                    else
                                        stockHistory.NewStock = stockHistory.LastStock + newStock;*/
                                }

                                productStockHistory.ProductStock = stockHistory.ProductStock;
                                productStockHistory.LastStock = stockHistory.LastStock;
                                productStockHistory.NewStock = stockHistory.NewStock;
                                productStockHistory.StockValue = stockHistory.StockValue;

                                if (productFinalStock != null)
                                {
                                    decimal totalStock = productStockHistory.NewStock;
                                    products.ForEach(obj => { if (obj.ProductId == productFinalStock.ProductId) obj.TotalStock = totalStock; });
                                }
                                else
                                {
                                    productFinalStock = new TotalInventory();
                                    productFinalStock.ProductId = productStockHistory.ProductId;
                                    productFinalStock.TotalStock = productStockHistory.NewStock;
                                    products.Add(productFinalStock);
                                }

                                productStockHistories.Add(productStockHistory);
                                //db.ProductStockHistory.Add(productStockHistory);
                            }
                        }
                    }

                    db.InventoryHistory.Add(inventoryHistory);
                    db.SaveChanges();

                    db.StockHistoryGroup.AddRange(stockHistoryGroups);
                    //stockHistoryGroups.ForEach(obj => { db.StockHistoryGroup.Add(obj); });
                    db.SaveChanges();

                    db.ProductStockHistory.AddRange(productStockHistories);
                    //productStockHistories.ForEach(obj => { db.ProductStockHistory.Add(obj); });
                    db.SaveChanges();

                    /*foreach (var product in products)
                    {
                        var _product = db.Product.FirstOrDefault(u => u.Id == product.ProductId);

                        if (_product.Unit == ProductUnit.Piece)
                            _product.StockQuantity = product.TotalStock;
                        else
                            _product.Weight = product.TotalStock;

                        _product.Updated = DateTime.Now;

                        db.Entry(_product).State = EntityState.Modified;
                    }*/

                    string json = JsonConvert.SerializeObject(model);
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    return httpResponseMessage;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new HttpResponseMessage(HttpStatusCode.ExpectationFailed) { Content = new StringContent(ex.ToString()) };
            }
        }

        public class BasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
        {
            public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
            {
                if (actionContext.Request.Headers.Authorization == null)
                {
                    actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }
                else
                {
                    string authToken = actionContext.Request.Headers.Authorization.Parameter;
                    string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));

                    string username = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                    string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);
                }
            }
        }
    }
}





