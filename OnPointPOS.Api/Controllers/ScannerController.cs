
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using POSSUM.Model;
using System.Web.Http;
using POSSUM.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using POSSUM.Api;
using POSSUM.Utils;
using System;
using System.Collections.Generic;

namespace POSSUM.Api.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    [System.Web.Http.RoutePrefix("api/Scanner")]
    public class ScannerController : BaseAPIController
    {
        string connectionString = "";
        bool nonAhenticated = false;

        public ScannerController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        /// <summary>
        /// API to POST Open Order
        /// </summary>
        /// <param name="order"> Order Model includes Order Detail List</param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostOpenOrder")]
        public IHttpActionResult PostOpenOrder(Order order)
        {
            try
            {
                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    var orderRepo = uof.OrderRepository;
                    var paymentRepo = uof.PaymentRepository;
                    var receiptRepo = uof.ReceiptRepository;
                    var orderLineRepo = uof.OrderLineRepository;
                    var invoicerepo = uof.InvoiceCounterRepository;

                    int lastNo = 0;
                    var existOrder = orderRepo.Where(o => o.Id == order.Id).FirstOrDefault();
                    if (existOrder == null)
                    {
                        var dt = order.CreationDate.Date;
                        var ord = orderRepo.Where(o => !string.IsNullOrEmpty(o.OrderNoOfDay) && o.OrderNoOfDay.StartsWith("SA") && o.CreationDate >= dt).OrderByDescending(o => o.CreationDate).FirstOrDefault();
                        if (ord != null)
                        {
                            string[] orNo = ord.OrderNoOfDay.Split('-');
                            if (orNo.Length > 1)
                                int.TryParse(orNo[1], out lastNo);
                        }

                        var endDate = dt.Day;
                        string OrderNoOfDay = "SA" + dt.Year.ToString() + dt.Month.ToString() + endDate + "-" + (lastNo + 1);

                        if (string.IsNullOrEmpty(order.InvoiceNumber))
                        {
                            long lastNo1 = 0;
                            var invoiceCounter = invoicerepo.FirstOrDefault(inc => inc.Id == 1);
                            if (invoiceCounter != null)
                            {
                                lastNo1 = Convert.ToInt64(invoiceCounter.LastNo);
                                invoiceCounter.LastNo = (lastNo1 + 1).ToString();
                            }
                            else
                            {
                                invoiceCounter = new InvoiceCounter
                                {
                                    Id = 1,
                                    LastNo = "1",
                                    InvoiceType = InvoiceType.Standard
                                };
                                invoicerepo.Add(invoiceCounter);
                            }
                            lastNo1 = lastNo1 + 1;
                            string invoiceNo = lastNo1 < 10 ? "00000" + lastNo1 : lastNo1 < 100 ? "0000" + lastNo1 : lastNo1 < 1000 ? "000" + lastNo1 : lastNo1 < 10000 ? "00" + lastNo1 : lastNo1 < 100000 ? "0" + lastNo1 : lastNo1.ToString();
                            order.InvoiceNumber = "SA-" + invoiceNo;
                        }

                        var lines = order.OrderLines;
                        order.OrderLines = null;
                        order.OrderSource = (int)OrderSource.ScannerApp;
                        order.OrderNoOfDay = OrderNoOfDay;
                        order.InvoiceGenerated = 1;
                        order.Status = OrderStatus.Completed;
                        order.InvoiceDate = order.CreationDate;
                        order.IsInInvoice = true;
                        orderRepo.AddOrUpdate(order);

                        var payment = new Payment()
                        {
                            OrderId = order.Id,
                            TypeId = 2,
                            Direction = order.OrderDirection,
                            PaidAmount = order.OrderTotal,
                            CashCollected = order.OrderTotal,
                            Id = Guid.NewGuid(),
                            PaymentDate = dt,
                            PaymentRef = "OnCredit"
                        };

                        paymentRepo.Add(payment);

                        long lastReceiptNo = 0;
                        try
                        {
                            lastReceiptNo = receiptRepo.Max(a => a.ReceiptNumber) + 1;
                        }
                        catch (Exception e)
                        {
                        }

                        var receipt = new Receipt()
                        {
                            ReceiptId = Guid.NewGuid(),
                            OrderId = order.Id,
                            PrintDate = dt,
                            ReceiptNumber = lastReceiptNo
                        };

                        receiptRepo.Add(receipt);

                        foreach (var orderLine in lines)
                        {
                            orderLine.Product = null;
                            orderLineRepo.AddOrUpdate(orderLine);
                        }

                        uof.Commit();
                        return Ok(new
                        {
                            Order = order,
                            Result = true,
                            Message = "Success"
                        });
                    }


                    return Ok(new
                    {
                        Order = existOrder,
                        Result = true,
                        Message = "Success"
                    });
                }
            }
            catch (System.Exception ex)
            {
                LogWriter.LogWrite(ex);
                //Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                //return StatusCode(HttpStatusCode.ExpectationFailed);
                return Ok(new
                {
                    Order = new Order(),
                    Result = false,
                    Message = ex.ToString()
                });
            }
        }

        public enum OrderSource
        {
            ScannerApp = 1
        }

        /// <summary>
        /// Get Product and categories updated in btween a date range
        /// </summary>
        /// <param name="dates">Last Sync Date, Curretn Date, Terminal Id</param>
        /// <returns>Model of ProductData that includes List of Products, Categories and Item Campaing , Accouting</returns>
        [HttpGet]
        [Route("SearchProducts")]
        public IHttpActionResult SearchProducts([FromUri] SeachProductModel dates)
        {
            try
            {
                var terminalId = dates.TerminalId;

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    int skip = dates.PageNo * dates.PageSize;
                    int take = dates.PageSize;
                    if (string.IsNullOrEmpty(dates.Term))
                        dates.Term = "";
                    dates.Term = dates.Term.ToLower();
                    string databaseName = db.Database.Connection.Database;
                    var terminal = db.Terminal.FirstOrDefault(t => t.Id == terminalId);
                    var outletId = terminal.Outlet.Id;
                    //var liveAccountings = db.Accounting.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveProducts = db.Product.Where(p => (p.BarCode != null && p.BarCode.ToLower().Contains(dates.Term)) || (p.Description != null && p.Description.Contains(dates.Term))).OrderByDescending(a => a.Updated).Skip(skip).Take(take).ToList();
                    //var liveCategories = db.Category.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    //var liveItemCampaigns = db.ProductCampaign.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    var liveProductPrices = db.ProductPrice.ToList();
                    //var livePricePolicy = db.Product_PricePolicy.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();

                    var liveItemCategories = new List<ItemCategory>();
                    var liveItemGroups = new List<ProductGroup>();

                    if (liveProducts.Count > 0)
                    {
                        foreach (var prod in liveProducts)
                        {
                            var itmCats = db.ItemCategory.Where(ic => ic.ItemId == prod.Id).ToList();
                            if (itmCats.Count > 0)
                                liveItemCategories.AddRange(itmCats);
                            var itemGroups = db.ProductGroup.Where(p => p.GroupId == prod.Id).ToList();
                            if (itemGroups.Count > 0)
                                liveItemGroups.AddRange(itemGroups);

                            var outletPrice = liveProductPrices.FirstOrDefault(p => p.ItemId == prod.Id && p.OutletId == outletId && p.PriceMode == PriceMode.Day);
                            if (outletPrice != null)
                                prod.Price = outletPrice.Price;
                        }
                    }

                    ProductData productData = new ProductData();
                    productData.Products = liveProducts;
                    //productData.Categories = liveCategories;
                    productData.ItemCategories = liveItemCategories;
                    //productData.Accountings = liveAccountings;
                    //productData.ProductCampaigns = liveItemCampaigns;
                    productData.ProductGroups = liveItemGroups;
                    //if (liveProductPrices.Count > 0)
                    //liveProductPrices = liveProductPrices.Where(p => p.Updated > LAST_EXECUTED_DATETIME && p.Updated <= EXECUTED_DATETIME).ToList();
                    productData.ProductPrices = liveProductPrices;
                    //productData.PricePolicies = livePricePolicy;
                    return Ok(new
                    {
                        Result = true,
                        Message = "Success",
                        Products = liveProducts
                    });
                }
            }
            catch (Exception ex)
            {
                //Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return Ok(new
                {
                    Result = false,
                    Message = ex.ToString(),
                    Products = new List<Product>()
                });
            }

        }

        [HttpPost]
        [Route("GetOrdersByGuid")]
        public async Task<List<Order>> GetOrdersByGuid(List<Guid> orderGuids)
        {
            try
            {
                List<Order> liveOrders = new List<Order>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var localOrders = db.OrderMaster.Include("OrderLines.Product").Where(o => orderGuids.Contains(o.Id)).ToList();
                    foreach (var order in localOrders)
                    {
                        var _order = new Order
                        {
                            Id = order.Id,
                            TableId = order.TableId,
                            CustomerId = order.CustomerId,
                            CreationDate = order.CreationDate,
                            OrderTotal = order.OrderTotal,
                            Status = order.Status,
                            UserId = order.UserId,
                            CheckOutUserId = order.CheckOutUserId,
                            ShiftClosed = order.ShiftClosed,
                            Comments = order.Comments,
                            TaxPercent = order.TaxPercent,
                            InvoiceDate = order.InvoiceDate,
                            InvoiceGenerated = order.InvoiceGenerated,
                            InvoiceNumber = order.InvoiceNumber,
                            OrderComments = order.OrderComments,
                            OrderNoOfDay = order.OrderNoOfDay,
                            PaymentStatus = order.PaymentStatus,
                            ShiftNo = order.ShiftNo,
                            ShiftOrderNo = order.ShiftOrderNo,
                            TipAmount = order.TipAmount,
                            TipAmountType = order.TipAmountType,
                            Updated = 0,
                            ZPrinted = order.ZPrinted,
                            Bong = order.Bong,
                            DailyBong = order.DailyBong,
                            Type = order.Type,
                            OutletId = order.OutletId,
                            TerminalId = order.TerminalId,
                            TrainingMode = order.TrainingMode,
                            RoundedAmount = order.RoundedAmount,
                            CustomerInvoiceId = order.CustomerInvoiceId,
                            OrderLines = order.OrderLines.ToList().Select(ol => new OrderLine(ol)).ToList()


                        };
                        liveOrders.Add(order);
                    }
                }
                return liveOrders;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new List<Order>();
            }
        }
    }
}

