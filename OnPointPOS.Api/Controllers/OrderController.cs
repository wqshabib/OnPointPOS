using System;
using System.Collections.Generic;
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

namespace POSSUM.Api.Controllers
{
    // [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    [System.Web.Http.RoutePrefix("api/Order")]
    public class OrderController : BaseAPIController
    {

        string connectionString = "";
        bool nonAhenticated = false;

        public OrderController()
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
                    var orderLineRepo = uof.OrderLineRepository;

                    var lines = order.OrderLines;
                    order.OrderLines = null;

                    orderRepo.AddOrUpdate(order);
                    foreach (var orderLine in lines)
                    {

                        orderLine.Product = null;

                        orderLineRepo.AddOrUpdate(orderLine);



                    }

                    uof.Commit();

                }


                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);

            }




        }

        /// <summary>
        /// Get Open Order along with Order Lines
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetOpenOrder")]
        public async Task<List<Order>> GetOpenOrder([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                Guid terminalId = dates.TerminalId;

                List<Order> liveOrders = new List<Order>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var localOrders = db.OrderMaster.Include("OrderLines.Product").Where(o => o.Status == OrderStatus.AssignedKitchenBar).ToList();
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

        /// <summary>
        /// Post Complete Order including payment and receipt info
        /// </summary>
        /// <param name="order"> Order Model, should include Orderlines, Payment detail and receipt info </param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostOrder")]
        public IHttpActionResult PostOrder(Order order)
        {
            try
            {
                LogWriter.LogWrite("PostOrder is calling....");
                LogWriter.LogWrite("PostOrder connectionstring: " + connectionString);

                var data = JsonConvert.SerializeObject(order);

                List<ProductStockHistory> stockHistoryToAdd = new List<ProductStockHistory>();

                LogWriter.LogWrite("PostOrder order recieved data: " + data);

                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    LogWriter.LogWrite("PostOrder order db is initialize: " + data);

                    var orderRepo = uof.OrderRepository;
                    var orderLineRepo = uof.OrderLineRepository;
                    var paymentRepo = uof.PaymentRepository;
                    var receiptRepo = uof.ReceiptRepository;
                    var prodRepo = uof.ProductRepository;
                    var catprodRepo = uof.ItemCategoryRepository;
                    var transactionRepo = uof.ItemTransactionRepository;
                    var outletRepo = uof.OutletRepository;
                    var lines = order.OrderLines;

                    order.OrderLines = null;

                    if (order.Outlet != null)
                    {
                        order.OutletId = order.Outlet.Id;
                        order.Outlet = null;
                    }

                    Guid warehouseId = default(Guid);

                    var outlet = outletRepo.FirstOrDefault(o => o.Id == order.OutletId);
                    if (outlet != null)
                        warehouseId = outlet.WarehouseID;

                    orderRepo.AddOrUpdate(order);

                    LogWriter.LogWrite("PostOrder order saved");

                    foreach (var orderLine in lines)
                    {
                        LogWriter.LogWrite("PostOrder order lines");

                        //this is the case when any product added at client terminal but yet not synced with live db
                        var product = prodRepo.FirstOrDefault(p => p.Id == orderLine.Product.Id);

                        if (product == null)
                        {
                            product = new Product()
                            {
                                Id = orderLine.Product.Id,
                                Description = orderLine.Product.Description,
                                Deleted = orderLine.Product.Deleted,
                                AccountingId = orderLine.Product.AccountingId,
                                Active = orderLine.Product.Active,
                                AskPrice = orderLine.Product.AskPrice,
                                AskWeight = orderLine.Product.AskWeight,
                                BarCode = orderLine.Product.BarCode,
                                Bong = orderLine.Product.Bong,
                                CategoryId = orderLine.Product.CategoryId,
                                ColorCode = orderLine.Product.ColorCode,
                                Created = orderLine.Product.Created,
                                PlaceHolder = orderLine.Product.PlaceHolder,
                                PLU = orderLine.Product.PLU,
                                Price = orderLine.Product.Price,
                                PriceLock = orderLine.Product.PriceLock,
                                PrinterId = orderLine.Product.PrinterId,
                                PurchasePrice = orderLine.Product.PurchasePrice,
                                Seamless = orderLine.Product.Seamless,
                                ShowItemButton = orderLine.Product.ShowItemButton,
                                SKU = orderLine.Product.SKU,
                                SortOrder = orderLine.Product.SortOrder,
                                Sticky = orderLine.Product.Sticky,
                                Tax = orderLine.Product.Tax,
                                Unit = orderLine.Product.Unit,
                                Updated = orderLine.Product.Updated,
                                ReceiptMethod = orderLine.Product.ReceiptMethod,
                                ItemType = orderLine.Product.ItemType,
                                DiscountAllowed = orderLine.Product.DiscountAllowed,
                                PreparationTime = orderLine.Product.PreparationTime
                            };

                            prodRepo.AddOrUpdate(product);

                            catprodRepo.Add(new ItemCategory
                            {
                                CategoryId = orderLine.Product.CategoryId,
                                ItemId = orderLine.Product.Id,
                                SortOrder = 1
                            });
                        }

                        orderLine.Product = null;

                        //Update StockQty
                        if ((order.Status == OrderStatus.ReturnOrder || order.Status == OrderStatus.Completed) && orderLine.Active != 0)
                        {
                            var productStockToAdjust = prodRepo.FirstOrDefault(p => p.Id == orderLine.ItemId);

                            if (productStockToAdjust != null)
                            {
                                ProductStockHistory productStockHistory = new ProductStockHistory();
                                productStockHistory.ProductId = orderLine.ItemId;

                                if (productStockToAdjust.Unit == ProductUnit.Piece)
                                {
                                    decimal productLastStock = productStockToAdjust.StockQuantity;
                                    decimal productNewStock = 0;

                                    if (order.Status == OrderStatus.Completed)
                                        productNewStock = productStockToAdjust.StockQuantity - orderLine.Quantity;
                                    else
                                        productNewStock = productStockToAdjust.StockQuantity + orderLine.Quantity;

                                    productStockToAdjust.StockQuantity = productNewStock;

                                    productStockHistory.LastStock = productLastStock;
                                    productStockHistory.NewStock = productNewStock;
                                }
                                else
                                {
                                    decimal? productLastWeight = (productStockToAdjust.Weight != null) ? productStockToAdjust.Weight : 0;
                                    decimal? productNewWeight = 0;

                                    if (order.Status == OrderStatus.Completed)
                                        productNewWeight = productLastWeight - orderLine.Quantity;
                                    else
                                        productNewWeight = productLastWeight + orderLine.Quantity;

                                    productStockToAdjust.Weight = productNewWeight;

                                    productStockHistory.LastStock = productLastWeight.Value;
                                    productStockHistory.NewStock = productNewWeight.Value;
                                }

                                prodRepo.AddOrUpdate(productStockToAdjust);

                                //list to add product stock history
                                stockHistoryToAdd.Add(productStockHistory);
                            }
                        }

                        orderLine.IsInventoryAdjusted = true;

                        //add item transaction entry
                        orderLineRepo.AddOrUpdate(orderLine);

                        //transactionRepo.AddOrUpdate(new ItemTransaction
                        //{
                        //	ItemTransactionID = Guid.NewGuid(),
                        //	ItemID = orderLine.ItemId,
                        //	OutletID = order.OutletId,
                        //	TerminalID = order.TerminalId,
                        //	Direction = orderLine.Direction,
                        //	OrderID = order.Id,
                        //	Qty = orderLine.Quantity,
                        //	TransactionDate = DateTime.Now,
                        //	WarehouseID = warehouseId
                        //});
                    }

                    foreach (var payment in order.Payments)
                    {
                        paymentRepo.AddOrUpdate(payment);
                    }

                    foreach (var recipt in order.Receipts)
                    {
                        receiptRepo.AddOrUpdate(recipt);
                    }

                    LogWriter.LogWrite("PostOrder order going to commit the uof");

                    try
                    {
                        uof.Commit();

                        if (stockHistoryToAdd.Count > 0)
                        {
                            using (var db = new ApplicationDbContext(connectionString))
                            {
                                foreach (var productStockHistory in stockHistoryToAdd)
                                {
                                    productStockHistory.Id = Guid.NewGuid();
                                    productStockHistory.CreatedOn = DateTime.Now;
                                    productStockHistory.UpdatedOn = DateTime.Now;

                                    db.ProductStockHistory.Add(productStockHistory);
                                }

                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogWrite("PostOrder order going to commit the uof exception" + ex.ToString());

                    }
                }

                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                LogWriter.LogWrite("Sync Order Fail:> Error Code::: " + ex.InnerException.ToString() + " : Message - exception fails");
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);

            }
        }

        /// <summary>
        /// Get cutomer inocvoices List
        /// </summary>
        /// <param name="dates">Start date, end date and terminal Id</param>
        /// <returns>List of CustomerInoice model</returns>
		[Route("GetCustomerInvoice")]
        public async Task<List<CustomerInvoice>> GetCustomerInvoice([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                Guid terminalId = dates.TerminalId;

                List<CustomerInvoice> liveCustomerInvoices = new List<CustomerInvoice>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {

                    var invoices = db.CustomerInvoice.Where(inv => inv.Synced == false && (inv.CreationDate >= LAST_EXECUTED_DATETIME && inv.CreationDate <= EXECUTED_DATETIME)).ToList();
                    foreach (var invoice in invoices)
                    {
                        var orders = db.OrderMaster.Where(c => c.CustomerInvoiceId == invoice.Id && c.TerminalId == terminalId).Select(o => new { o.Id }).ToList();
                        invoice.OrdersGuid = new List<Guid>();
                        foreach (var or in orders)
                        {
                            invoice.OrdersGuid.Add(or.Id);
                        }
                        if (invoice.OrdersGuid.Count > 0)
                            liveCustomerInvoices.Add(invoice);
                    }
                }
                return liveCustomerInvoices;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new List<CustomerInvoice>();
            }
        }

        /// <summary>
        /// Post Customer Inocise
        /// </summary>
        /// <param name="invoice">Inoice model with list of orders linke with invoice</param>
        /// <returns></returns>
		[HttpPost]
        [Route("PostCustomerInvoice")]
        public IHttpActionResult PostCustomerInvoice(CustomerInvoice invoice)
        {
            try
            {

                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var invoiceRepo = uof.CustomerInvoiceRepository;
                    var orderRepo = uof.OrderRepository;
                    invoice.Synced = true;
                    // invoice.PaymentStatus
                    invoiceRepo.AddOrUpdate(invoice);
                    if (invoice.OrdersGuid != null)
                    {
                        foreach (var orderId in invoice.OrdersGuid)
                        {
                            var order = orderRepo.FirstOrDefault(o => o.Id == orderId);
                            if (order != null)
                            {
                                order.CustomerInvoiceId = invoice.Id;
                                orderRepo.AddOrUpdate(order);
                            }
                        }
                    }
                    uof.Commit();


                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }
        /// <summary>
        /// Post z-reports
        /// </summary>
        /// <param name="terminalStatus"> Terminal StatusLog includiing infor of Report and Report data</param>
        /// <returns></returns>
		[HttpPost]
        [Route("PostReports")]
        public IHttpActionResult PostReports(TerminalStatusLog terminalStatus)
        {
            try
            {

                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var terminalStatusRepo = uof.TerminalStatusLogRepository;
                    var reportDataRepo = uof.ReportDataRepository;
                    var reportRepo = uof.ReportRepository;

                    terminalStatusRepo.AddOrUpdate(terminalStatus);
                    if (terminalStatus.Report != null)
                    {

                        reportRepo.AddOrUpdate(terminalStatus.Report);
                    }
                    foreach (var reportData in terminalStatus.ReportData)
                    {
                        reportDataRepo.AddOrUpdate(reportData);
                    }
                    uof.Commit();


                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }
        /// <summary>
        /// Post Cash Drawer Log
        /// </summary>
        /// <param name="log">Cash Drawer Log Mode</param>
        /// <returns></returns>
		[HttpPost]
        [Route("PostCashDrawerLog")]
        public IHttpActionResult PostCashDrawerLog(CashDrawerLog log)
        {
            try
            {

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var _log = db.CashDrawerLog.FirstOrDefault(cl => cl.Id == log.Id);
                    if (_log == null)
                    {
                        db.CashDrawerLog.Add(log);
                        db.Entry(log.CashDrawer).State = System.Data.Entity.EntityState.Unchanged;
                    }

                    db.SaveChanges();

                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }

        //    [HttpDelete]
        //    public IHttpActionResult Delete([FromUri] string id)
        //    {

        //        if (_session == null || _session.IsOpen == false)
        //        {
        //            Init();
        //        }

        //        try
        //        {
        //            var repo = new Repository<Order, int>(_session);

        //            var product = repo.Get(int.Parse(id));

        //            product.Deleted = true;

        //            _session.SaveOrUpdate(product);

        //            _session.Transaction.Commit();

        //            _session.Close();

        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode(HttpStatusCode.InternalServerError);
        //        }

        //        return StatusCode(HttpStatusCode.OK);
        //    }
        //}

        //public class BasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
        //{
        //    public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        //    {
        //        if (actionContext.Request.Headers.Authorization == null)
        //        {
        //            actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        //        }
        //        else
        //        {
        //            string authToken = actionContext.Request.Headers.Authorization.Parameter;
        //            string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));

        //            string username = decodedToken.Substring(0, decodedToken.IndexOf(":"));
        //            string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);
        //        }
        //    }
        //}

        [HttpGet]
        [Route("GetOrdersWithoutExternalID")]
        public async Task<List<Order>> GetOrdersWithoutExternalID([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                DateTime EXECUTED_DATETIME = dates.CurrentDate;
                Guid terminalId = dates.TerminalId;
                var pageNo = dates.PageNo;
                var pagesize = dates.PageSize;
                var pgNo = pageNo * pagesize;
                List<Order> liveOrders = new List<Order>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var localOrders = db.OrderMaster.Include("OrderLines.Product").Where(o => o.TerminalId == terminalId && string.IsNullOrEmpty(o.ExternalID) && o.Status == OrderStatus.Completed).OrderBy(o => o.CreationDate).Skip(pgNo).Take(pagesize).ToList();
                    foreach (var order in localOrders)
                    {
                        var customer = db.Customer.FirstOrDefault(f => f.Id == order.CustomerId);
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
                            Type = order.Type,
                            OutletId = order.OutletId,
                            TerminalId = order.TerminalId,
                            TrainingMode = order.TrainingMode,
                            RoundedAmount = order.RoundedAmount,
                            CustomerInvoiceId = order.CustomerInvoiceId,
                            OrderLines = order.OrderLines.ToList().Select(ol => new OrderLine(ol)).ToList(),
                            Customer = customer

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

        [HttpPost]
        [Route("PostOrderExternalInformation")]
        public IHttpActionResult PostOrderExternalInformation(Order order)
        {
            try
            {
                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var orderObj = uof.OrderRepository.FirstOrDefault(a => a.Id == order.Id);
                    if (orderObj != null)
                    {
                        orderObj.ExternalID = order.ExternalID;
                        uof.Commit();
                    }
                }

                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetInviceGeneratedOpenOrder")]
        public async Task<List<Order>> GetInviceGeneratedOpenOrder([FromUri] Dates dates)
        {
            try
            {
                LogWriter.LogWrite("GetInviceGeneratedOpenOrder calling ");

                List<Order> liveOrders = new List<Order>();
                var pageNo = dates.PageNo;
                var pagesize = dates.PageSize;
                Guid terminalId = dates.TerminalId;
                var pgNo = pageNo * pagesize;
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var localOrders = db.OrderMaster.Include("OrderLines.Product").Where(o => o.TerminalId == terminalId && (o.CustomerId != null && o.CustomerId != Guid.Empty) && !o.IsVismaInvoiceGenerated).OrderBy(o => o.CreationDate).Skip(pgNo).Take(pagesize).ToList().ToList();
                    foreach (var order in localOrders)
                    {
                        var customer = db.Customer.FirstOrDefault(c => c.Id == order.CustomerId);

                        order.Customr = new Customer
                        {
                            Id = customer.Id,
                            Active = customer.Active,
                            Name = customer.Name,
                            Email = customer.Email,
                            City = customer.City,
                            Address1 = customer.Address1,
                            ZipCode = customer.ZipCode,
                            Address2 = customer.Address2,
                            CustomerNo = customer.CustomerNo,
                            Phone = customer.Phone,
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


        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateInviceGeneratedOpenOrder")]
        public IHttpActionResult UpdateInviceGeneratedOpenOrder(List<Order> orders)
        {
            try
            {
                LogWriter.LogWrite("UpdateInviceGeneratedOpenOrder calling...");

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    foreach (var order in orders)
                    {
                        var oldOrder = db.OrderMaster.FirstOrDefault(o => o.Id == order.Id);
                        if (oldOrder != null)
                        {
                            oldOrder.IsVismaInvoiceGenerated = true;
                            oldOrder.ExternalID = order.ExternalID;
                            db.Entry(oldOrder).State = System.Data.Entity.EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("UpdateInviceGeneratedOpenOrder exception..." + ex.ToString());
                LogWriter.LogWrite(ex);
                LogWriter.SendEmail(ex.ToString());
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }




        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateOrderStatus/{orderId}/{status}")]
        public IHttpActionResult UpdateOrderStatus(string orderId, int status)
        {
            try
            {
                LogWriter.LogWrite("UpdateOrderStatus calling...");
                Guid orderGuid = Guid.Parse(orderId);
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var oldOrder = db.OrderMaster.FirstOrDefault(o => o.Id == orderGuid);
                    if (oldOrder != null)
                    {
                        oldOrder.Status = (OrderStatus)status;
                        oldOrder.Updated = 1;
                        db.Entry(oldOrder).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("UpdateOrderStatus exception..." + ex.ToString());
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }



        [HttpPost]
        [Route("PostOnlineOrder")]
        public IHttpActionResult PostOnlineOrder(Order order)
        {

            try
            {
                LogWriter.LogWrite("PostOrder is calling....");
                LogWriter.LogWrite("PostOrder connectionstring: " + connectionString);
                var data = JsonConvert.SerializeObject(order);
                LogWriter.LogWrite("PostOrder order recieved data: " + data);


                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    LogWriter.LogWrite("PostOrder order db is initialize: " + data);

                    var orderRepo = uof.OrderRepository;
                    var orderLineRepo = uof.OrderLineRepository;
                    var paymentRepo = uof.PaymentRepository;
                    var receiptRepo = uof.ReceiptRepository;
                    var prodRepo = uof.ProductRepository;
                    var catprodRepo = uof.ItemCategoryRepository;
                    var transactionRepo = uof.ItemTransactionRepository;
                    var outletRepo = uof.OutletRepository;
                    var lines = order.OrderLines;
                    order.OrderLines = null;
                    if (order.Outlet != null)
                    {
                        order.OutletId = order.Outlet.Id;
                        order.Outlet = null;
                    }
                    Guid warehouseId = default(Guid);
                    var outlet = outletRepo.FirstOrDefault(o => o.Id == order.OutletId);
                    if (outlet != null)
                        warehouseId = outlet.WarehouseID;
                    //added by khalil 2021-12-33 orderintId in order for dibs 
                    if (order.OrderIntID == null || order.OrderIntID == 0)
                    {
                        var orderintId = orderRepo.Where(o => o.OrderIntID != null).Max(o => (int?)o.OrderIntID);
                        order.OrderIntID = orderintId == null ? 1 : orderintId + 1;
                    }
                    if (string.IsNullOrEmpty(order.OrderNoOfDay))
                    {
                        var ordDate = DateTime.Now.Date;
                        int lastNo = 0;
                        var ord = orderRepo.GetAll().OrderByDescending(o => o.CreationDate).FirstOrDefault(o => o.OrderNoOfDay != "" && o.CreationDate >= ordDate);
                        if (ord != null)
                        {
                            if (ord.OrderNoOfDay != null)
                            {
                                string[] orNo = ord.OrderNoOfDay.Split('-');
                                if (orNo.Length > 1)
                                    int.TryParse(orNo[1], out lastNo);
                            }

                        }
                        order.OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day + "-" + (lastNo + 1);

                    }

                    orderRepo.AddOrUpdate(order);

                    LogWriter.LogWrite("PostOrder order saved");


                    foreach (var orderLine in lines)
                    {
                        LogWriter.LogWrite("PostOrder order lines");

                        //this is the case when any product added at client terminal but yet not synced with live db
                        var product = prodRepo.FirstOrDefault(p => p.Id == orderLine.Product.Id);
                        if (product == null)
                        {

                            product = new Product()
                            {
                                Id = orderLine.Product.Id,
                                Description = orderLine.Product.Description,
                                Deleted = orderLine.Product.Deleted,
                                AccountingId = orderLine.Product.AccountingId,
                                Active = orderLine.Product.Active,
                                AskPrice = orderLine.Product.AskPrice,
                                AskWeight = orderLine.Product.AskWeight,
                                BarCode = orderLine.Product.BarCode,
                                Bong = orderLine.Product.Bong,
                                CategoryId = orderLine.Product.CategoryId,
                                ColorCode = orderLine.Product.ColorCode,
                                Created = orderLine.Product.Created,
                                PlaceHolder = orderLine.Product.PlaceHolder,
                                PLU = orderLine.Product.PLU,
                                Price = orderLine.Product.Price,
                                PriceLock = orderLine.Product.PriceLock,
                                PrinterId = orderLine.Product.PrinterId,
                                PurchasePrice = orderLine.Product.PurchasePrice,
                                Seamless = orderLine.Product.Seamless,
                                ShowItemButton = orderLine.Product.ShowItemButton,
                                SKU = orderLine.Product.SKU,
                                SortOrder = orderLine.Product.SortOrder,
                                Sticky = orderLine.Product.Sticky,
                                Tax = orderLine.Product.Tax,
                                Unit = orderLine.Product.Unit,
                                Updated = orderLine.Product.Updated,
                                ReceiptMethod = orderLine.Product.ReceiptMethod,
                                ItemType = orderLine.Product.ItemType,
                                DiscountAllowed = orderLine.Product.DiscountAllowed,
                                PreparationTime = orderLine.Product.PreparationTime,

                            };

                            prodRepo.AddOrUpdate(product);
                            catprodRepo.Add(new ItemCategory
                            {
                                CategoryId = orderLine.Product.CategoryId,
                                ItemId = orderLine.Product.Id,
                                SortOrder = 1
                            });
                        }
                        orderLine.Product = null;

                        orderLineRepo.AddOrUpdate(orderLine);
                        //add item transaction entry

                        //update StockQty
                        if (orderLine.Active != 0)
                        {

                            var itm = prodRepo.FirstOrDefault(p => p.Id == orderLine.ItemId);
                            if (itm != null)
                            {
                                itm.StockQuantity = itm.StockQuantity - (orderLine.Direction * orderLine.Quantity);
                                prodRepo.AddOrUpdate(itm);
                            }
                        }
                        //transactionRepo.AddOrUpdate(new ItemTransaction
                        //{
                        //	ItemTransactionID = Guid.NewGuid(),
                        //	ItemID = orderLine.ItemId,
                        //	OutletID = order.OutletId,
                        //	TerminalID = order.TerminalId,
                        //	Direction = orderLine.Direction,
                        //	OrderID = order.Id,
                        //	Qty = orderLine.Quantity,
                        //	TransactionDate = DateTime.Now,
                        //	WarehouseID = warehouseId

                        //});



                    }
                    foreach (var payment in order.Payments)
                    {
                        paymentRepo.AddOrUpdate(payment);

                    }
                    foreach (var recipt in order.Receipts)
                    {
                        receiptRepo.AddOrUpdate(recipt);

                    }
                    LogWriter.LogWrite("PostOrder order going to commit the uof");
                    try
                    {
                        uof.Commit();

                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogWrite("PostOrder order going to commit the uof exception" + ex.ToString());

                    }

                }


                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                LogWriter.LogWrite("Sync Order Fail:> Error Code::: " + ex.InnerException.ToString() + " : Message - exception fails");
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);

            }




        }






        [HttpGet]
        [Route("GetOrdersHistory/{customerid}")]
        public async Task<List<Order>> GetOrdersHistory(string customerid)
        {
            try
            {

                List<Order> liveOrders = new List<Order>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    Guid customerGuid = Guid.Parse(customerid);
                    var localOrders = db.OrderMaster.Include("OrderLines.Product").Where(o => o.IsOnlineOrder && o.CustomerId == customerGuid).OrderBy(o => o.CreationDate).ToList();
                    foreach (var order in localOrders)
                    {
                        var customer = db.Customer.FirstOrDefault(f => f.Id == order.CustomerId);
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
                            Type = order.Type,
                            OutletId = order.OutletId,
                            TerminalId = order.TerminalId,
                            TrainingMode = order.TrainingMode,
                            RoundedAmount = order.RoundedAmount,
                            CustomerInvoiceId = order.CustomerInvoiceId,
                            OrderLines = order.OrderLines.ToList().Select(ol => new OrderLine(ol)).ToList(),
                            Customer = customer,
                            DeliveryDate = order.DeliveryDate == null ? DateTime.Now : order.DeliveryDate,
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



        [HttpGet]
        [Route("GetCustomerCartOrder/{customerid}")]
        public async Task<Order> GetCustomerCartOrder(string customerid)
        {
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    Guid customerGuid = Guid.Parse(customerid);
                    var order = db.OrderMaster.Include("OrderLines.Product").FirstOrDefault(o => o.IsOnlineOrder && o.CustomerId == customerGuid && o.Status == OrderStatus.New && o.PaymentStatus == 0);


                    if (order != null)
                    {
                        var customer = db.Customer.FirstOrDefault(f => f.Id == order.CustomerId);
                        return new Order
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
                            Type = order.Type,
                            OutletId = order.OutletId,
                            TerminalId = order.TerminalId,
                            TrainingMode = order.TrainingMode,
                            RoundedAmount = order.RoundedAmount,
                            CustomerInvoiceId = order.CustomerInvoiceId,
                            OrderLines = order.OrderLines.ToList().Select(ol => new OrderLine(ol)).ToList(),
                            Customer = customer,
                            OrderIntID = order.OrderIntID,
                            DeliveryDate = order.DeliveryDate

                        };
                    }
                    else
                        return new Order();
                }

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return new Order();
            }
        }






    }
}



