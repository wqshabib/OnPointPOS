using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
    public class OrderController
    {
        string connectionString;
        public OrderController(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Order> GetUpdatedOrder(Guid terminalId, Guid outletId)
        {
            try
            {
                List<Order> preparedOrder = new List<Order>();

                using (var db = new ApplicationDbContext(connectionString))
                {
                    var localOrders = db.OrderMaster.Include("Payments").Include("Receipts").Include("OrderLines.Product").Where(o => o.Updated == 1 && o.InvoiceGenerated == 1 && o.TrainingMode == false).Take(50).ToList();
                    Log.WriteLog("Total local Orders:" + localOrders.Count().ToString());
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
                            DailyBong= order.DailyBong,
                            Type = order.Type,
                            OutletId = outletId,
                            TerminalId = order.TerminalId,
                            TrainingMode = order.TrainingMode,
                            RoundedAmount = order.RoundedAmount,
                            CustomerInvoiceId = order.CustomerInvoiceId,                            
                            Payments = db.Payment.Where(a => a.OrderId == order.Id).ToList().Select(p=>new Payment(p)).ToList(),
                            Receipts = db.Receipt.Where(a=>a.OrderId==order.Id).ToList().Select(r=>new Receipt(r)).ToList(),
                            OrderLines = db.OrderDetail.Where(a => a.OrderId == order.Id).ToList().Select(ol=>new OrderLine(ol)).ToList()


                        };
                        /*
                        var lines = _order.OrderLines.Where(o => o.OrderId == order.Id).Select(ol => new OrderLine
                        {
                            Id = ol.Id,
                            Product = new Product { Id = ol.Product.Id, Description = ol.Product.Description, BarCode = ol.Product.BarCode, PLU = ol.Product.PLU, Tax = ol.Product.Tax, PurchasePrice = ol.Product.PurchasePrice, Price = ol.Product.Price, ShowItemButton = ol.Product.ShowItemButton },
                            ItemId = ol.Product.Id,
                            OrderId = ol.OrderId,
                            UnitPrice = ol.UnitPrice,
                            Quantity = ol.Quantity,
                            ItemDiscount = ol.ItemDiscount,
                            Direction = ol.Direction,
                            Active = ol.Active,
                            DiscountedUnitPrice = ol.DiscountedUnitPrice,
                            DiscountPercentage = ol.DiscountPercentage,
                            IsCoupon = ol.IsCoupon,
                            TaxPercent = ol.TaxPercent,
                            ItemComments = ol.ItemComments,
                            ItemStatus = ol.ItemStatus,
                            PurchasePrice = ol.PurchasePrice,
                            UnitsInPackage = ol.UnitsInPackage,
                            WebDetailId = ol.WebDetailId,
                            DiscountType = ol.DiscountType,
                            DiscountDescription = ol.DiscountDescription,
                            ItemType = ol.ItemType,
                            GroupId = ol.GroupId,                     
                            GroupKey = ol.GroupKey,
                            IngredientMode = ol.IngredientMode


                        }).ToList();
                        //_order.OrderLines = lines;
                        */
                        if (_order.Payments.Count == 0 || _order.Receipts.Count == 0)
                        {
                            continue;
                        }

                        preparedOrder.Add(_order);

                    }
                }
                return preparedOrder;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return new List<Order>();

            }
        }

        internal List<TerminalStatusLog> GetReportData(Guid terminalId)
        {
            List<TerminalStatusLog> preparedReports = new List<TerminalStatusLog>();
            try
            {


                using (var db = new ApplicationDbContext(connectionString))
                {

                    var terminal = db.Terminal.FirstOrDefault(tr => tr.Id == terminalId);
                    var terminalStatuses = db.TerminalStatusLog.Where(o => o.Synced == 0 && (o.ReportId != null && o.ReportId != default(Guid))).ToList();

                    foreach (var status in terminalStatuses)
                    {
                        var Report = db.Report.FirstOrDefault(r => r.Id == status.ReportId);
                        var _terminalstatus = new TerminalStatusLog
                        {
                            Id = status.Id,
                            TerminalId = status.TerminalId,
                            Report = new Report
                            {
                                Id = Report.Id,
                                CreationDate = Report.CreationDate,
                                TerminalId = Report.TerminalId,
                                ReportType = Report.ReportType,
                                ReportNumber = Report.ReportNumber
                            },
                            ActivityDate = status.ActivityDate,
                            UserId = status.UserId,
                            Status = status.Status,
                            ReportId = status.ReportId,
                            Synced = status.Synced,
                            ReportData = db.ReportData.Where(r => r.ReportId == status.ReportId).ToList()
                        };
                        preparedReports.Add(_terminalstatus);

                    }

                }


            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());


            }
            return preparedReports;
        }
        public void UpdateReportStatus(Guid id)
        {

            using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
            {
                var localTerminalStatusRepo = uofLocal.TerminalStatusLogRepository;
                var _terminalstatus = localTerminalStatusRepo.FirstOrDefault(s => s.Id == id);
                if (_terminalstatus != null)
                {
                    _terminalstatus.Synced = 1;
                    localTerminalStatusRepo.AddOrUpdate(_terminalstatus);
                }
                uofLocal.Commit();
            }

        }
        public void UpdateOrderStatus(Guid orderId)
        {
            try
            {

                using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var localOrderRepo = uofLocal.OrderRepository;
                    var localOrder = localOrderRepo.FirstOrDefault(o => o.Id == orderId);
                    localOrder.Updated = 0;
                    localOrderRepo.AddOrUpdate(localOrder);
                    uofLocal.Commit();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
            }
        }
        public bool UpdateOrder()
        {
            try
            {
                List<Order> preparedOrder = new List<Order>();
                using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var localOrderRepo = uofLocal.OrderRepository;
                    var localOrderLineRepo = uofLocal.OrderLineRepository;
                    var localPyamentRepo = uofLocal.PaymentRepository;
                    var localReceiptRepo = uofLocal.ReceiptRepository;
                    var localOrders = localOrderRepo.Where(o => o.Updated == 1 && o.InvoiceGenerated == 1 && o.TrainingMode == false).ToList();

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
                            OutletId = order.Outlet.Id,
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
                            Type = order.Type,
                            TerminalId = order.TerminalId,
                            TrainingMode = order.TrainingMode,
                            RoundedAmount = order.RoundedAmount,
                            CustomerInvoiceId = order.CustomerInvoiceId,
                            OrderLines = localOrderLineRepo.Where(o => o.OrderId == order.Id).Select(ol => new OrderLine
                            {
                                Id = ol.Id,
                                Product = new Product { Id = ol.Product.Id, Description = ol.Product.Description, BarCode = ol.Product.BarCode, PLU = ol.Product.PLU, Tax = ol.Product.Tax, PurchasePrice = ol.Product.PurchasePrice, Price = ol.Product.Price, ShowItemButton = ol.Product.ShowItemButton },
                                ItemId = ol.Product.Id,
                                OrderId = ol.OrderId,
                                UnitPrice = ol.UnitPrice,
                                Quantity = ol.Quantity,
                                ItemDiscount = ol.ItemDiscount,
                                Direction = ol.Direction,
                                Active = ol.Active,
                                DiscountedUnitPrice = ol.DiscountedUnitPrice,
                                DiscountPercentage = ol.DiscountPercentage,
                                IsCoupon = ol.IsCoupon,
                                TaxPercent = ol.TaxPercent,
                                ItemComments = ol.ItemComments,
                                ItemStatus = ol.ItemStatus,
                                PurchasePrice = ol.PurchasePrice,
                                UnitsInPackage = ol.UnitsInPackage,
                                DiscountType = ol.DiscountType,
                                DiscountDescription = ol.DiscountDescription,
                                ItemType = ol.ItemType,
                                GroupId = ol.GroupId,
                            }).ToList(),
                            Payments = localPyamentRepo.Where(p => p.OrderId == order.Id).ToList(),
                            Receipts = localReceiptRepo.Where(r => r.OrderId == order.Id).ToList()

                        };
                        preparedOrder.Add(_order);

                    }

                }
                if (preparedOrder.Count > 0)
                {

                    //using (IUnitOfWork uofLive = PosState.GetInstance().CreateLiveUnitOfWork())
                    //{
                    //    var liveOrdersRepo = new Repository<Order, Guid>(uofLive.Session);
                    //    var liveOrderLinesRepo = new Repository<OrderLine, Guid>(uofLive.Session);
                    //    var liveReceiptRepo = new Repository<Receipt, Guid>(uofLive.Session);
                    //    var livePaymentRepo = new Repository<Payment, Guid>(uofLive.Session);

                    //    foreach (var order in preparedOrder)
                    //    {
                    //        if (order.OrderLines.Count == 0 || order.Terminal == null)
                    //            continue;

                    //        var _order = liveOrdersRepo.FirstOrDefault(o => o.Id == order.Id);
                    //        if (_order == null)
                    //        {
                    //            // liveOrdersRepo.Add(order);
                    //            IDbCommand command = new SqlCommand();
                    //            command.Connection = uofLive.Session.Connection;
                    //            command.CommandType = CommandType.Text;
                    //            command.CommandText = OrderInsertSql(order);

                    //            uofLive.Session.Transaction.Enlist(command);

                    //            command.ExecuteNonQuery();

                    //            foreach (var orderLine in order.OrderLines)
                    //            {
                    //                liveOrderLinesRepo.Add(orderLine);
                    //            }
                    //            foreach (var payment in order.Payments)
                    //            {
                    //                livePaymentRepo.Add(payment);
                    //            }
                    //            foreach (var recipt in order.Receipts)
                    //            {
                    //                liveReceiptRepo.Add(recipt);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            _order.SumAmt = order.SumAmt;
                    //            _order.Status = order.Status;
                    //            _order.CheckOutMacId = order.CheckOutMacId;
                    //            _order.CheckOutUserId = order.CheckOutUserId;
                    //            _order.ShiftClosed = order.ShiftClosed;
                    //            _order.Comments = order.Comments;
                    //            _order.InvoiceDate = order.InvoiceDate;
                    //            _order.InvoiceGenerated = order.InvoiceGenerated;
                    //            _order.InvoiceNumber = order.InvoiceNumber;
                    //            _order.PaymentStatus = order.PaymentStatus;

                    //        }
                    //    }
                    //    uofLive.Commit();
                    // }

                    using (IUnitOfWork uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
                    {
                        var localOrderRepo = uofLocal.OrderRepository;


                        foreach (var order in preparedOrder)
                        {
                            if (order.OrderLines.Count == 0)
                                continue;
                            var localOrder = localOrderRepo.FirstOrDefault(o => o.Id == order.Id);
                            localOrder.Updated = 0;
                            localOrderRepo.AddOrUpdate(localOrder);
                        }
                        uofLocal.Commit();
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;

            }
        }

        public void UploadInvoiceCounter(string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                using (var db = new ApplicationDbContext(connectionString))
                {
                    var lst = db.InvoiceCounter.ToList();
                    foreach (var model in lst)
                    {
                        ServiceClient client = new ServiceClient(baseUrl, apiUser, apiPassword);
                        client.PostInvoiceCounter(model);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("UploadInvoiceCounter Error : " + ex.ToString());
            }
        }

        public void UpdateInvoiceCounter(List<InvoiceCounter> lst)
        {
            try
            {
                using (var db = new ApplicationDbContext(connectionString))
                {
                    foreach (var model in lst)
                    {
                        var lastNo = Convert.ToInt64(model.LastNo);
                        var obj = db.InvoiceCounter.FirstOrDefault(a => a.InvoiceType == model.InvoiceType);
                        if (obj != null && Convert.ToInt64(obj.LastNo) < lastNo)
                        {
                            obj.LastNo = Convert.ToString(lastNo);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("UpdateInvoiceCounter Error : " + ex.ToString());
            }
        }

        /// <summary>
        /// This method will be called on completion of order when clinet terminal running in online mode
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool UpdateOrder(Guid orderId)
        {
            try
            {
                List<Order> preparedOrder = new List<Order>();

                using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var localOrderRepo = uofLocal.OrderRepository;
                    var localOrderLineRepo = uofLocal.OrderLineRepository;
                    var localPyamentRepo = uofLocal.PaymentRepository;
                    var localReceiptRepo = uofLocal.ReceiptRepository;
                    var order = localOrderRepo.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
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
                            OrderLines = localOrderLineRepo.Where(ol => ol.OrderId == order.Id).ToList(),
                            Type = order.Type,
                            Outlet = order.Outlet,
                            TerminalId = order.TerminalId,
                            TrainingMode = order.TrainingMode,
                            RoundedAmount = order.RoundedAmount,
                            Payments = localPyamentRepo.GetAll().Where(p => p.OrderId == order.Id).ToList(),
                            Receipts = localReceiptRepo.GetAll().Where(r => r.OrderId == order.Id).ToList()

                        };
                        preparedOrder.Add(_order);

                    }

                }
                if (preparedOrder.Count > 0)
                {


                    // var helper = new NHibernateHelper(ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString).SessionFactory;

                    using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
                    {
                        var localOrderRepo = uofLocal.OrderRepository;


                        foreach (var order in preparedOrder)
                        {
                            if (order.OrderLines.Count == 0)
                                continue;
                            var localOrder = localOrderRepo.FirstOrDefault(o => o.Id == order.Id);
                            localOrder.Updated = 0;
                        }
                        uofLocal.Commit();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;

            }
        }

        public bool UpdateZReport()
        {
            try
            {
                List<TerminalStatusLog> preparedReports = new List<TerminalStatusLog>();

                using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var localTerminalStatusRepo = uofLocal.TerminalStatusLogRepository;
                    var localReportDataRepo = uofLocal.ReportDataRepository;
                    var terminalStatuses = localTerminalStatusRepo.Where(o => o.Synced == 0 && (o.ReportId != null && o.ReportId != default(Guid))).ToList();
                    foreach (var status in terminalStatuses)
                    {

                        var _terminalstatus = new TerminalStatusLog
                        {
                            Id = status.Id,
                            Terminal = status.Terminal,
                            ActivityDate = status.ActivityDate,
                            UserId = status.UserId,
                            Status = status.Status,
                            ReportId = status.ReportId,
                            Synced = status.Synced,
                            ReportData = localReportDataRepo.Where(r => r.ReportId == status.ReportId).ToList()
                        };
                        preparedReports.Add(_terminalstatus);

                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;

            }
        }

        public List<CustomerInvoice> GetCustomerInvoices()
        {
            List<CustomerInvoice> invoices = new List<CustomerInvoice>();

            using (var db = new ApplicationDbContext(connectionString))
            {
                invoices = db.CustomerInvoice.Where(inv => inv.Synced == false).ToList();
                foreach(var invoic in invoices)
                {
                    var orders = db.OrderMaster.Where(c => c.CustomerInvoiceId == invoic.Id).Select(o => new  { o.Id }).ToList();
                    invoic.OrdersGuid = new List<Guid>();
                    foreach (var or in orders)
                    {
                        invoic.OrdersGuid.Add(or.Id);
                    }
                }
            }

            return invoices;
        }

        public void UpdateInvoiceStatus(Guid id)
        {

            using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
            {
                var invoiceRepo = uofLocal.CustomerInvoiceRepository;
                var _invoice = invoiceRepo.FirstOrDefault(s => s.Id == id);
                if (_invoice != null)
                {
                    _invoice.Synced = true;
                    invoiceRepo.AddOrUpdate(_invoice);
                }
                uofLocal.Commit();
            }

        }



        public void UpateCustomerInvoice(List<CustomerInvoice> invoices)
        {
            try
            {
                if (invoices != null && invoices.Count > 0)
                {
                    using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
                    {
                        var invoiceRepo = uof.CustomerInvoiceRepository;
                        var orderRepo = uof.OrderRepository;
                        foreach (CustomerInvoice invoice in invoices)
                        {
                            invoice.Synced = true;
                            invoiceRepo.AddOrUpdate(invoice);
                            if (invoice.OrdersGuid != null)
                            {
                                foreach (var orderId in invoice.OrdersGuid)
                                {
                                    var order = orderRepo.FirstOrDefault(o => o.Id == orderId);
                                    order.CustomerInvoiceId = invoice.Id;
                                    orderRepo.AddOrUpdate(order);
                                }
                            }
                        }
                        uof.Commit();


                    }
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog("Download Invoice: "+ex.Message);
            }
        }
    }
}
