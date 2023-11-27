using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using POSSUM.Data;
using POSSUM.Integration;
using POSSUM.Model;
using System.Globalization;

namespace POSSUM.Handlers
{
    public class InvoiceHandler
    {

        public Order GetOrderMaster(Guid orderId)
        {
            try
            {
                return new OrderRepository(PosState.GetInstance().Context).GetOrderMaster(orderId);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new Order();
            }
        }



        public Receipt GetOrderReceipt(Guid orderId)
        {
            try
            {
                return new OrderRepository(PosState.GetInstance().Context).GetOrderReceipt(orderId);

            }
            catch (Exception ex)
            {
                // LogWriter.LogWrite(ex);
                return new Receipt();
            }
        }
        public List<Setting> GetReceiptBankSettings()
        {
            try
            {
                return new SettingRepository().GetReceiptBankSettings();

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new List<Setting>();
            }
        }

        internal Guid GenerateCustomerInvoiceSingleOrder(Guid OrderId, Guid customerId)
        {
            return new CustomerInvoiceRepository().GenerateCustomerInvoiceSingleOrder(OrderId, customerId);
        }
        internal Guid GenerateCustomerInvoiceMultipleOrders(List<Order> orders, Guid customerId, string remanrks)
        {

            return new CustomerInvoiceRepository().GenerateCustomerInvoice(orders, customerId, remanrks);
        }

        public POSSUM.ViewModels.CustomerInvoiceModel GetCustomerInvoice2(Guid InvoiceId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    POSSUM.ViewModels.CustomerInvoiceModel model = new POSSUM.ViewModels.CustomerInvoiceModel();


                    var invoice = db.CustomerInvoice.FirstOrDefault(inv => inv.Id == InvoiceId);
                    if (invoice != null)
                    {
                        model.Invoice = invoice;
                        var customer = db.Customer.FirstOrDefault(cust => cust.Id == invoice.CustomerId);
                        if (customer != null)
                            model.Customer = customer;
                        //Detail


                        var orderLines = (from ordLine in db.OrderDetail.Where(o => o.Active == 1 && o.IsCoupon != 1)
                                          join ord in db.OrderMaster on ordLine.OrderId equals ord.Id
                                          where ord.CustomerInvoiceId == InvoiceId
                                          select new POSSUM.ViewModels.OrderLineViewModel
                                          {
                                              Id = ordLine.Id,
                                              OrderId = ordLine.OrderId,
                                              OutletId = ord.OutletId,
                                              Product = ordLine.Product,
                                              ItemId = ordLine.Product.Id,
                                              ItemName = ordLine.Product.Description,
                                              Quantity = ordLine.Quantity,
                                              UnitPrice = ordLine.UnitPrice,
                                              UnitsInPackage = ordLine.UnitsInPackage,
                                              DiscountedUnitPrice = ordLine.DiscountedUnitPrice,
                                              DiscountPercentage = ordLine.DiscountPercentage,
                                              Direction = ordLine.Direction,
                                              Active = ordLine.Active,
                                              PurchasePrice = ordLine.PurchasePrice,
                                              IsCoupon = ordLine.IsCoupon,
                                              ItemDiscount = ordLine.ItemDiscount,
                                              ItemComments = ordLine.ItemComments,
                                              ItemStatus = ordLine.ItemStatus,
                                              TaxPercent = ordLine.TaxPercent,
                                              DiscountType = ordLine.DiscountType,
                                              DiscountDescription = ordLine.DiscountDescription


                                          }).ToList();



                        var groups = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice }).ToList();
                        List<POSSUM.ViewModels.OrderLineViewModel> lines = new List<POSSUM.ViewModels.OrderLineViewModel>();
                        foreach (var group in groups)
                        {
                            var ordLine = group.First();
                            var grossTotal = group.Sum(s => s.GrossAmountDiscounted());
                            var netTotal = group.Sum(s => s.NetAmount());
                            var qty = group.Sum(s => (s.Direction * s.Quantity));
                            lines.Add(new POSSUM.ViewModels.OrderLineViewModel
                            {
                                Id = ordLine.Id,
                                OutletId = ordLine.OutletId,
                                Product = ordLine.Product,
                                ItemName = ordLine.ItemName,
                                Quantity = qty,
                                UnitPrice = ordLine.UnitPrice,
                                UnitsInPackage = ordLine.UnitsInPackage,
                                DiscountedUnitPrice = ordLine.DiscountedUnitPrice,
                                DiscountPercentage = ordLine.DiscountPercentage,
                                Direction = 1,
                                Active = ordLine.Active,
                                PurchasePrice = ordLine.PurchasePrice,
                                IsCoupon = ordLine.IsCoupon,
                                ItemDiscount = ordLine.ItemDiscount,
                                ItemComments = ordLine.ItemComments,
                                ItemStatus = ordLine.ItemStatus,
                                TaxPercent = ordLine.TaxPercent,
                                GrossTotal = grossTotal,// ordLine.GrossAmount(),

                            });

                        }
                        model.OrderDetails = lines.OrderBy(o => o.ItemName).ToList();

                    }

                    return model;
                }
            }
            catch (Exception ex)
            {
                return new POSSUM.ViewModels.CustomerInvoiceModel();
            }
        }



        public CustomerInvoiceModel GetCustomerInvoice(Guid InvoiceId)
        {
            try
            {
                return new CustomerInvoiceRepository().GetCustomerInvoice(InvoiceId);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return new CustomerInvoiceModel();
            }
        }
        public List<OrderLine> GetOrderDetailForCustomerInvoiceById(Guid customerInvoiceId)
        {

            try
            {
                return new CustomerInvoiceRepository().GetOrderDetailForCustomerInvoiceById(customerInvoiceId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new List<OrderLine>();
            }
        }

        public List<Customer> GetAllCustomers()
        {
            try
            {
                return new CustomerRepository().GetAllCustomers();

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new List<Customer>();
            }
        }

        public Customer GetCustomer(Guid customerId)
        {
            try
            {
                return new CustomerRepository().GetCustomerById(customerId);

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new Customer();
            }
        }





        public bool SaveVoucherTransaction(VoucherTransaction transaction, string productSKU)
        {

            using (var db = new ApplicationDbContext())
            {

                var product = db.Product.First(p => p.SKU == productSKU);
                transaction.Id = Guid.NewGuid();
                transaction.Product = product;
                db.VoucherTransaction.Add(transaction);
                db.SaveChanges();
                return true;
            }
        }
        public bool SaveOrderPayment(Payment payment, decimal roundAmount)
        {
            LogWriter.LogWrite("Rounding issue " + roundAmount.ToString());


            if (roundAmount >= 1)
                roundAmount = 0;

            using (var uof = new UnitOfWork(new ApplicationDbContext()))
            {
                var orderRepo = uof.OrderRepository;
                var paymentRepo = uof.PaymentRepository;
                var journalRepo = uof.JournalRepository;
                var order = orderRepo.First(o => o.Id == payment.OrderId);
                var ordertotal = order.OrderTotal;
                decimal paid = 0;
                var payments = paymentRepo.AsQueryable().Where(p => p.OrderId == payment.OrderId && p.Direction == 1 && payment.TypeId != 7).ToList();
                if (payments.Count > 0)
                {
                    paid = payments.Sum(p => p.PaidAmount);
                    if (paid >= order.OrderTotal)
                        return true;
                }

                //var alreadyentry = paymentRepo.FirstOrDefault(p => p.Order.Id == payment.OrderId && p.PaymentType.Id == payment.TypeId && p.PaidAmount == ordertotal);
                //if (alreadyentry != null)
                //    return true;
                // For cash and swish
                if (payment.TypeId == 1 || payment.TypeId == 10) // munir... if fracPart is exist then save rounding else 0
                {
                    long intPart = (long)ordertotal;
                    decimal fracPart = ordertotal - intPart;
                    // if (fracPart>0)
                    order.RoundedAmount = roundAmount;
                }


                LogWriter.LogWrite("order.Id " + order.Id.ToString() + " OrderTotal " + order.OrderTotal.ToString() + " RoundedAmount" + order.RoundedAmount);

                payment.OrderId = order.Id;
                payment.Id = Guid.NewGuid();
                paymentRepo.Add(payment);
                paid = paid + payment.PaidAmount;
                if (paid < ordertotal)
                    order.PaymentStatus = 2;
                if (payment.TypeId == 0 || payment.TypeId == 1 || payment.TypeId == 2 || payment.TypeId == 4 || payment.TypeId == 7 || payment.TypeId == 9 || payment.TypeId == 10)
                {
                    //0 coupon, 1 cash, 2 account, 3 Gifft, 4 CreditCard, 5 DebitCard, 7 cashback, 9 mobile, 10 swish
                    int actionId = payment.TypeId == 0 ? Convert.ToInt16(JournalActionCode.OrderCouponPayment) : payment.TypeId == 1 ? Convert.ToInt16(JournalActionCode.OrderCashPayment) : payment.TypeId == 2 ? Convert.ToInt16(JournalActionCode.OrderAccountPayment) : payment.TypeId == 4 ? Convert.ToInt16(JournalActionCode.OrderCreditcardPayment) : payment.TypeId == 7 ? Convert.ToInt16(JournalActionCode.OrderReturnCashPayment) : payment.TypeId == 9 ? Convert.ToInt16(JournalActionCode.OrderMobileTerminalPayment) : Convert.ToInt16(JournalActionCode.OrderSwishPayment);
                    var journal = new Journal
                    {
                        ActionId = actionId,
                        Created = DateTime.Now,
                        OrderId = order.Id,
                        UserId = Defaults.User.Id,
                        TerminalId = order.TerminalId


                    };
                    journalRepo.Add(journal);
                }
                orderRepo.AddOrUpdate(order);
                uof.Commit();
                return true;
            }


        }
        public bool SaveTipMode(Guid orderId, DateTime paymentDate, int paymentType, string paymentRef, decimal tipAmount)
        {
            using (var db = new ApplicationDbContext())
            {

                var payment = new Payment
                {
                    PaymentDate = DateTime.Now,
                    PaymentRef = paymentRef,
                    TipAmount = tipAmount,
                    OrderId = orderId,
                    TypeId = paymentType
                };
                db.Payment.Add(payment);
                db.SaveChanges();
            }
            return true;
        }

        public bool SaveCashDrawLog(CashDrawerLog log)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    log.Id = Guid.NewGuid();

                    db.CashDrawerLog.Add(log);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return false;
            }
        }

        public bool GenerateInvoiceLocal(Guid orderId, int ShiftNo, string checkOutUserId, PaymentTransactionStatus creditcardPaymentResult)
        {
            LogWriter.CheckOutLogWrite("Generating receipt", orderId);
            var order = new Order();// GetOrderMasterDetailById(orderId);
            var orderDetails = new List<OrderLine>();
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReceiptGenerating), orderId, null, null, null);
            using (var uof = new UnitOfWork(new ApplicationDbContext()))
            {

                var invoiceCounterRepo = uof.InvoiceCounterRepository;
                long lastNo = 0;
                var ordRepo = uof.OrderRepository;
                var paymentRepo = uof.PaymentRepository;
                order = ordRepo.FirstOrDefault(o => o.Id == orderId);
                //  var totalPaid = paymentRepo.Where(o => o.OrderId == orderId).Sum(s => s.PaidAmount);
                //if (  totalPaid<order.OrderTotal)
                //    return false;
                if (order.CustomerId == default(Guid))
                {
                    lastNo = Defaults.ReceiptCounter + 1;
                }
                else
                {
                    var customerBonusRepo = uof.CustomerBonusRepository;
                    var ordDetailRepo = uof.OrderLineRepository;
                    var details = ordDetailRepo.Where(o => o.OrderId == orderId && o.Active == 1 && o.ItemType != ItemType.Grouped).ToList();
                    var orderTotal = details.Sum(s => s.GrossAmount());
                    decimal cahngeAmount = orderTotal - order.OrderTotal;
                    var customerBonus = new CustomerBonus
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = order.CustomerId,
                        OutletId = Defaults.Outlet.Id,
                        ChangeValue = cahngeAmount,
                        CurrentSum = order.OrderTotal,
                        CreatedOn = DateTime.Now,
                        OrderId = orderId,

                    };
                    customerBonusRepo.Add(customerBonus);
                    var invoiceCounter = invoiceCounterRepo.FirstOrDefault(inc => inc.Id == 1);

                    if (invoiceCounter != null)
                    {
                        lastNo = Convert.ToInt64(invoiceCounter.LastNo);
                        invoiceCounter.LastNo = (lastNo + 1).ToString();
                    }
                    else
                    {
                        invoiceCounter = new InvoiceCounter
                        {
                            Id = 1,
                            LastNo = "1",
                            InvoiceType = InvoiceType.Standard
                        };

                    }
                    invoiceCounterRepo.AddOrUpdate(invoiceCounter);
                    lastNo = lastNo + 1;
                }

                string invoiceNo = lastNo < 10 ? "00000" + lastNo : lastNo < 100 ? "0000" + lastNo : lastNo < 1000 ? "000" + lastNo : lastNo < 10000 ? "00" + lastNo : lastNo < 100000 ? "0" + lastNo : lastNo.ToString();

                var ordLineRepo = uof.OrderLineRepository;

                if (order != null)
                {
                    LogWriter.CheckOutLogWrite("Assining Invoice Number", orderId);
                    order.InvoiceNumber = invoiceNo;
                    if (order.PaymentStatus != 3)
                    {
                        order.PaymentStatus = 1;
                        LogWriter.CheckOutLogWrite("Setting Payment status as Paid", orderId);
                    }
                    //order.InvoiceDate = DateTime.Now;
                    //order.InvoiceGenerated = 1;
                    //if (order.Status != OrderStatus.ReturnOrder)
                    //    order.Status = OrderStatus.Completed;
                    //order.Updated = 1;
                    //order.CheckOutUserId = checkOutUserId;
                    orderDetails = ordLineRepo.Where(ol => ol.OrderId == orderId && ol.Active == 1 && ol.ItemType != ItemType.Grouped).ToList();
                    ordRepo.AddOrUpdate(order);
                }
                uof.Commit();
            }
            var vatAmounts = new List<VAT>();

            LogWriter.CheckOutLogWrite("Calculating VAT for Receipt", orderId);

            decimal vatPercent = 0;
            var taxPerDetails = orderDetails.Where(t => t.TaxPercent == vatPercent).ToList();
            decimal Tot = taxPerDetails.Sum(tot => tot.GrossAmountDiscounted());
            decimal net = VAT.NetFromGross(Tot, vatPercent);
            decimal vat = Tot - net;
            vatAmounts.Add(new VAT(vatPercent, vat));

            vatPercent = 6;
            taxPerDetails = orderDetails.Where(t => t.TaxPercent == vatPercent).ToList();
            Tot = taxPerDetails.Sum(tot => tot.GrossAmountDiscounted());
            net = VAT.NetFromGross(Tot, vatPercent);
            vat = Tot - net;
            vatAmounts.Add(new VAT(vatPercent, vat));

            vatPercent = 12;
            taxPerDetails = orderDetails.Where(t => t.TaxPercent == vatPercent).ToList();
            Tot = taxPerDetails.Sum(tot => tot.GrossAmountDiscounted());
            net = VAT.NetFromGross(Tot, vatPercent);
            vat = Tot - net;
            vatAmounts.Add(new VAT(vatPercent, vat));

            vatPercent = 25;
            taxPerDetails = orderDetails.Where(t => t.TaxPercent == vatPercent).ToList();
            Tot = taxPerDetails.Sum(tot => tot.GrossAmountDiscounted());
            net = VAT.NetFromGross(Tot, vatPercent);
            vat = Tot - net;
            vatAmounts.Add(new VAT(vatPercent, vat));
            var receiptRepo = new ReceiptHandler();
            var receipt = receiptRepo.FromOrderMaster(order, vatAmounts, Defaults.Terminal.Id, creditcardPaymentResult);
            if (order.Status == OrderStatus.ReturnOrder)
            {
                receipt.NegativeAmount = orderDetails.Where(d => d.Direction == -1 && d.Active == 1).Sum(s => ((s.Quantity * s.UnitPrice) + s.ItemDiscount));
            }
            else
            {
                if (orderDetails.Where(s => s.Active == 1 && ((s.Quantity * s.UnitPrice) + s.ItemDiscount) < 0).Count() > 0)
                {
                    receipt.NegativeAmount = orderDetails.Where(s => s.Active == 1 && ((s.Quantity * s.UnitPrice) + s.ItemDiscount) < 0).Sum(s => ((s.Quantity * s.UnitPrice) + s.ItemDiscount));
                    receipt.GrossAmount = orderDetails.Where(s => s.Active == 1 && ((s.Quantity * s.UnitPrice) + s.ItemDiscount) > 0).Sum(s => ((s.Quantity * s.UnitPrice) + s.ItemDiscount));
                }
            }
            LogWriter.CheckOutLogWrite("Saving Receipt data", orderId);
            receiptRepo.Create(receipt);
            bool receiptGenerated = false;

            /* Start control code  */
            var cuAction = PosState.GetInstance().ControlUnitAction;


            var aa = cuAction.ControlUnit.CheckStatus();

            cuAction.ControlUnit.CheckStatus();

            cuAction.ControlUnit.RegisterPOS(Defaults.CompanyInfo.OrgNo, Defaults.Terminal.TerminalNo.ToString());

            // LogWriter.CheckOutLogWrite("Sending receipt to control unit", orderId);
            //   Integration.ControlUnits.CleanCash.CleanCashControlUnit2 cleanCash=new Integration.ControlUnits.CleanCash.CleanCashControlUnit2();


            int attempts = 1;
            var x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, 1); // true for Clean cash debugging
            while (x == null && attempts < 4)
            {
                attempts++;
                x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, attempts); // true for Clean cash debugging
            }

            MessageBoxResult messageBoxResult = MessageBoxResult.Cancel;

            if (x == null)
            {
                var lastFailedOrder = new OrderRepository(PosState.GetInstance().Context).GetOrderByStatusFirstOrDefault(new List<OrderStatus>() { OrderStatus.CleanCashFailed, OrderStatus.CleanCashReturnOrderFailed }, Guid.Parse(Defaults.TerminalId));
                if (lastFailedOrder.Count > 3)
                {
                    messageBoxResult = MessageBox.Show("Clean cash is not responding, press Ok to retry.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    while (messageBoxResult == MessageBoxResult.OK)
                    {
                        x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, ++attempts); // true for Clean cash debugging
                        if (x == null)
                        {
                            messageBoxResult = MessageBox.Show("Clean cash is not responding, press Ok to retry.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (x == null)
            {
                if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    var lastSuccessReceipt = receiptRepo.GetLastReceipt();
                    if (lastSuccessReceipt != null)
                    {
                        //x = new ControlUnitResponse(true, Defaults.CONTROL_UNIT_FAILED_NAME, "1234567890abcdefghijklmnopqrstuvwxyz");
                        x = new ControlUnitResponse(true, lastSuccessReceipt.ControlUnitName, "");
                    }
                    else
                    {
                        LogWriter.LogWrite("No receipt is available yet.");
                    }
                }
            }

            try
            {
                if (x != null)
                {
                    receipt.ControlUnitCode = x.ControlCode;
                    receipt.ControlUnitName = x.UnitName;
                    //if (Defaults.ControlUnitType == ControlUnitType.CLEAN_CASH)
                    //{
                    if (string.IsNullOrEmpty(x.ControlCode))
                    {
                        LogWriter.LogWrite("Control Unit failed to register POS");
                        LogWriter.CheckOutLogWrite("Control Unit failed to register POS", orderId);
                        // Don;t need to delete receipt
                        //receiptRepo.Delete(receipt);
                        //throw new ControlUnitException("Control Unit Failed to get Code ", null);
                    }
                    else
                    {
                        LogWriter.CheckOutLogWrite(string.Format("Control Unit Code= {0}, and Control Unit Name= {1}", x.ControlCode, x.UnitName), orderId);
                    }
                    //}
                    //cuAction.ControlUnit.Close();
                    receiptGenerated = true;
                }
                //else
                //{

                //    receiptRepo.Delete(receipt);
                //    //if (Defaults.ControlUnitType == ControlUnitType.CLEAN_CASH)
                //    //{
                //    App.MainWindow.ShowError("CONTROL UNIT FAILURE", "Send receipt to controlunit failed ");
                //    LogWriter.CheckOutLogWrite("Send receipt to controlunit failed", orderId);
                //    LogWriter.LogWrite("Send receipt to controlunit failed ");
                //    // }
                //}



            }
            catch (Exception ex)
            {

                cuAction = PosState.GetInstance().ControlUnitAction;

                var status = cuAction.ControlUnit.CheckStatus();

                PosState.GetInstance().ReTryControlUnit();
                cuAction = PosState.GetInstance().ControlUnitAction;
                status = cuAction.ControlUnit.CheckStatus();

                if (status != ControlUnitStatus.OK)
                    throw new Exception(ex.Message);
                else
                {
                    attempts = 1;
                    x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, 1); // true for Clean cash debugging
                    while (x == null && attempts < 4)
                    {
                        attempts++;
                        x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, attempts); // true for Clean cash debugging
                    }

                    // Assigned a dummy code and unit name to avoid removing this from database
                    if (x == null)
                    {
                        var lastFailedOrder = new OrderRepository(PosState.GetInstance().Context).GetOrderByStatusFirstOrDefault(new List<OrderStatus>() { OrderStatus.CleanCashFailed, OrderStatus.CleanCashReturnOrderFailed }, Guid.Parse(Defaults.TerminalId));
                        if (lastFailedOrder.Count > 3)
                        {
                            messageBoxResult = MessageBox.Show("Clean cash is not responding, press Ok to retry.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            while (messageBoxResult == MessageBoxResult.OK)
                            {
                                x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, ++attempts); // true for Clean cash debugging
                                if (x == null)
                                {
                                    messageBoxResult = MessageBox.Show("Clean cash is not responding, press Ok to retry.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (x == null)
                        {
                            if (messageBoxResult == MessageBoxResult.Cancel)
                            {
                                var lastSuccessReceipt = receiptRepo.GetLastReceipt();
                                if (lastSuccessReceipt != null)
                                {
                                    //x = new ControlUnitResponse(true, Defaults.CONTROL_UNIT_FAILED_NAME, "1234567890abcdefghijklmnopqrstuvwxyz");
                                    x = new ControlUnitResponse(true, lastSuccessReceipt.ControlUnitName, "");
                                }
                                else
                                {
                                    LogWriter.LogWrite("No receipt is available yet.");
                                }
                            }
                        }
                    }

                    if (x != null)
                    {
                        receipt.ControlUnitCode = x.ControlCode;
                        receipt.ControlUnitName = x.UnitName;
                        //if (Defaults.ControlUnitType == ControlUnitType.CLEAN_CASH)
                        //{
                        if (string.IsNullOrEmpty(x.ControlCode))
                        {
                            LogWriter.LogWrite("Control Unit failed to register POS");
                            LogWriter.CheckOutLogWrite("Control Unit failed to register POS", orderId);

                            // Don;t need to delete receipt
                            //receiptRepo.Delete(receipt);
                            //throw new ControlUnitException("Control Unit Failed to get Code ", null);
                        }
                        else
                        {
                            LogWriter.CheckOutLogWrite(string.Format("Control Unit Code= {0}, and Control Unit Name= {1}", x.ControlCode, x.UnitName), orderId);
                        }
                        //}
                        //cuAction.ControlUnit.Close();
                        receiptGenerated = true;
                    }
                    else
                    {

                        //receiptRepo.Delete(receipt);
                        //if (Defaults.ControlUnitType == ControlUnitType.CLEAN_CASH)
                        //{
                        App.MainWindow.ShowError("CONTROL UNIT FAILURE", "Send receipt to controlunit failed ");
                        LogWriter.CheckOutLogWrite("Send receipt to controlunit failed", orderId);
                        LogWriter.LogWrite("Send receipt to controlunit failed ");
                        throw new ControlUnitException(ex.Message, null);

                        // }
                    }
                }

                ////receiptRepo.Delete(receipt);
                //LogWriter.CheckOutLogWrite(ex.Message, orderId);
                //throw new ControlUnitException(ex.Message, null);
            }

            if (receiptGenerated)
            {
                LogWriter.CheckOutLogWrite("After successfull rceipt sent to control Unit,  updating order info", orderId);

                using (var uof = new UnitOfWork(new ApplicationDbContext()))
                {
                    var ordRepo = uof.OrderRepository;
                    var journalRepo = uof.JournalRepository;
                    var _order = ordRepo.FirstOrDefault(o => o.Id == orderId);
                    _order.InvoiceDate = DateTime.Now;
                    _order.InvoiceGenerated = 1;
                    LogWriter.CheckOutLogWrite("Marking order status as completed", orderId);

                    if (string.IsNullOrEmpty(receipt.ControlUnitCode))
                    {
                        if (_order.Status != OrderStatus.ReturnOrder)
                            _order.Status = OrderStatus.CleanCashFailed;
                        else
                            _order.Status = OrderStatus.CleanCashReturnOrderFailed;
                    }
                    else
                    {
                        if (_order.Status != OrderStatus.ReturnOrder)
                            _order.Status = OrderStatus.Completed;
                    }

                    _order.Updated = 1;
                    _order.CheckOutUserId = checkOutUserId;
                    var journal = new Journal
                    {
                        OrderId = orderId,
                        ActionId = Convert.ToInt16(JournalActionCode.ReceiptGenerated),
                        Created = DateTime.Now,
                        UserId = Defaults.User.Id,
                        TerminalId = order.TerminalId
                    };
                    journalRepo.Add(journal);
                    uof.Commit();
                }
                LogWriter.CheckOutLogWrite("Updating receipt with control unit  code and name", orderId);
                receiptRepo.Update(receipt);
                LogWriter.CheckOutLogWrite("Receipt generated successfully", orderId);

            }
            else
            {

                receiptRepo.Delete(receipt);
            }
            /* End control code */
            // return true;

            #region // If any receipt is processed, then lets see if there is any failed receipt available
            LogWriter.LogWrite("Checking if failed orders can be processed or not. Current control code = " + x.ControlCode);
            if (!string.IsNullOrEmpty(x.ControlCode))
            {
                LogWriter.LogWrite("ProcessFailedOrders. checkOutUserId = " + checkOutUserId);                
                ProcessFailedOrders(checkOutUserId, null);
            }
            else
            {
                LogWriter.LogWrite("ProcessFailedOrders. Can not process failed orders as current order is also failed = " + order.Id);
            }

            #endregion

            return receiptGenerated;
        }

        public void RetryOrder(Order order)
        {
            try
            {
                ProcessFailedOrders(Defaults.User.Id, order.Id);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        public enum FailedOrdersCCStatus
        { 
            OrderFoundAndSuccess, OrderNotFound, OrderFoundButFailed
        }
        Random _random = new Random();
        public FailedOrdersCCStatus ProcessFailedOrders(string checkOutUserId, Guid? orderId)
        {
            FailedOrdersCCStatus foundOrder = FailedOrdersCCStatus.OrderNotFound;
            try
            {
                using (var uof = new UnitOfWork(new ApplicationDbContext()))
                {
                    var ordRepo = uof.OrderRepository;
                    var journalRepo = uof.JournalRepository;
                    LogWriter.LogWrite("ProcessFailedOrders Started");
                    var cuAction = PosState.GetInstance().ControlUnitAction;
                    var aa = cuAction.ControlUnit.CheckStatus();
                    cuAction.ControlUnit.CheckStatus();
                    cuAction.ControlUnit.RegisterPOS(Defaults.CompanyInfo.OrgNo, Defaults.Terminal.TerminalNo.ToString());
                    var receiptRepo = new ReceiptHandler();
                    Receipt receiptToPrcess = null;
                    LogWriter.LogWrite("ProcessFailedOrders=> Current Order Id = " + orderId);
                    if (orderId == null)
                    {
                        LogWriter.LogWrite("ProcessFailedOrders=> Order is null, loading last failed order.");
                        var lastFailedOrder = ordRepo.GetOrderByStatusFirstOrDefault(new List<OrderStatus>() { OrderStatus.CleanCashFailed, OrderStatus.CleanCashReturnOrderFailed }, Guid.Parse(Defaults.TerminalId));
                        if (lastFailedOrder != null && lastFailedOrder.Count > 0)
                        {
                            //LogWriter.LogWrite("Sleep=> Start : " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                            //System.Threading.Thread.Sleep(1000);
                            //LogWriter.LogWrite("Sleep=> end : " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                            LogWriter.LogWrite("ProcessFailedOrders=> Found last failed orders " + lastFailedOrder.Count);
                            int index = _random.Next(0, lastFailedOrder.Count - 1);
                            orderId = lastFailedOrder[index].Id;
                            LogWriter.LogWrite("ProcessFailedOrders=> Last failed Order Id = " + orderId +" , at index= "+ index);
                        }
                    }
                    if (orderId != null)
                    {
                        LogWriter.LogWrite("ProcessFailedOrders=> Loading receipt for Order Id = " + orderId);
                        receiptToPrcess = receiptRepo.GetByOrderId(orderId.Value);
                    }

                    if (receiptToPrcess != null)
                    {
                        var lines = ordRepo.GetOrderLinesByOrder(receiptToPrcess.OrderId);
                        if (lines.Where(s => s.Active == 1 && ((s.Quantity * s.UnitPrice) + s.ItemDiscount) < 0).Count() > 0)
                        {
                            receiptToPrcess.NegativeAmount = lines.Where(s => s.Active == 1 && ((s.Quantity *  s.UnitPrice) + s.ItemDiscount) < 0).Sum(s => ((s.Quantity * s.UnitPrice) + s.ItemDiscount));
                            receiptToPrcess.GrossAmount = lines.Where(s => s.Active == 1 && ((s.Quantity * s.UnitPrice) + s.ItemDiscount) > 0).Sum(s => ((s.Quantity *  s.UnitPrice) + s.ItemDiscount));
                        }

                        foundOrder = FailedOrdersCCStatus.OrderFoundButFailed;
                        LogWriter.LogWrite("ProcessFailedOrders - Found a receipt to process with id = " + receiptToPrcess.ReceiptId);

                        var attempts = 1;
                        LogWriter.LogWrite("ProcessFailedOrders - Sending receipt attempt number = " + attempts);
                        var y = cuAction.ControlUnit.SendReceipt(receiptToPrcess, Defaults.User, 1); // true for Clean cash debugging
                        while (y == null && attempts < 4)
                        {
                            attempts++;
                            LogWriter.LogWrite("ProcessFailedOrders - Sending receipt attempt number = " + attempts);
                            y = cuAction.ControlUnit.SendReceipt(receiptToPrcess, Defaults.User, attempts); // true for Clean cash debugging
                        }

                        if (y != null)
                        {
                            if (string.IsNullOrEmpty(y.ControlCode))
                            {
                                foundOrder = FailedOrdersCCStatus.OrderFoundButFailed;

                                LogWriter.LogWrite("ProcessFailedOrders - Control Unit failed to register POS");
                                LogWriter.CheckOutLogWrite("ProcessFailedOrders - Control Unit failed to register POS", receiptToPrcess.OrderId);
                            }
                            else
                            {
                                foundOrder = FailedOrdersCCStatus.OrderFoundAndSuccess;

                                LogWriter.CheckOutLogWrite(string.Format("ProcessFailedOrders - Control Unit Code= {0}, and Control Unit Name= {1}", y.ControlCode, y.UnitName), receiptToPrcess.OrderId);
                                LogWriter.CheckOutLogWrite("ProcessFailedOrders - After successfull rceipt sent to control Unit,  updating order info", receiptToPrcess.OrderId);
                                LogWriter.LogWrite("ProcessFailedOrders - After successfull rceipt sent to control Unit,  updating order info " + receiptToPrcess.OrderId);

                                receiptToPrcess.ControlUnitCode = y.ControlCode;
                                receiptToPrcess.ControlUnitName = y.UnitName;

                                var _order = ordRepo.FirstOrDefault(o => o.Id == receiptToPrcess.OrderId);
                                if (_order != null)
                                {
                                    LogWriter.LogWrite("ProcessFailedOrders - After successfull rceipt sent to control Unit,  Order loaded " + receiptToPrcess.OrderId);
                                    if (_order.InvoiceDate == null)
                                        _order.InvoiceDate = DateTime.Now;
                                    _order.InvoiceGenerated = 1;
                                    LogWriter.CheckOutLogWrite("Marking order status as completed", receiptToPrcess.OrderId);
                                    LogWriter.LogWrite("ProcessFailedOrders - Marking order status as completed. Current Status: " + _order.Status);
                                    if (_order.Status != OrderStatus.CleanCashReturnOrderFailed)
                                        _order.Status = OrderStatus.Completed;
                                    else
                                        _order.Status = OrderStatus.ReturnOrder;
                                    LogWriter.LogWrite("ProcessFailedOrders - Marking order status as completed. New Status: " + _order.Status);

                                    _order.Updated = 1;
                                    _order.CheckOutUserId = checkOutUserId;
                                    var journal = new Journal
                                    {
                                        OrderId = receiptToPrcess.OrderId,
                                        ActionId = Convert.ToInt16(JournalActionCode.ReceiptGenerated),
                                        Created = DateTime.Now,
                                        UserId = Defaults.User.Id,
                                        TerminalId = _order.TerminalId
                                    };
                                    journalRepo.Add(journal);
                                    LogWriter.LogWrite("ProcessFailedOrders - Commiting changes.");
                                    uof.Commit();
                                }
                            }
                            LogWriter.CheckOutLogWrite("ProcessFailedOrders - Updating receipt with control unit  code and name", receiptToPrcess.OrderId);
                            receiptRepo.Update(receiptToPrcess);
                            LogWriter.LogWrite("ProcessFailedOrders - Updating receipt with control unit  code and name "+ receiptToPrcess.OrderId);
                            LogWriter.CheckOutLogWrite("ProcessFailedOrders - Receipt generated successfully", receiptToPrcess.OrderId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

            return foundOrder;
        }

        public int LoadFailedOrdersCount()
        {
            try
            {
                using (var uof = new UnitOfWork(new ApplicationDbContext()))
                {
                    var ordRepo = uof.OrderRepository;
                    
                    var lastFailedOrder = ordRepo.GetOrderByStatusFirstOrDefault(new List<OrderStatus>() { OrderStatus.CleanCashFailed, OrderStatus.CleanCashReturnOrderFailed }, Guid.Parse(Defaults.TerminalId));

                    return lastFailedOrder.Count;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

            return 0;
        }

        public bool ReGenerateInvoice(Guid orderId, string checkOutUserId, Receipt receipt)
        {
            bool receiptGenerated = false;
            /* Start control code  */
            using (var cuAction = PosState.GetInstance().ControlUnitAction)
            {
                cuAction.ControlUnit.CheckStatus();
                cuAction.ControlUnit.RegisterPOS(Defaults.CompanyInfo.OrgNo, Defaults.Terminal.TerminalNo.ToString());
                int attempts = 1;
                var x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, 1); // true for Clean cash debugging
                while (x == null && attempts < 4)
                {
                    attempts++;
                    x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, attempts); // true for Clean cash debugging
                }

                // Assigned a dummy code and unit name to avoid removing this from database
                if (x == null)
                {
                    x = new ControlUnitResponse(true, "CONTROL_UNIT_FAILED", "1234567890abcdefghijklmnopqrstuvwxyz");
                }

                if (x != null)
                {
                    receipt.ControlUnitCode = x.ControlCode;
                    receipt.ControlUnitName = x.UnitName;

                    cuAction.ControlUnit.Close();
                    receiptGenerated = true;
                }
                else
                    LogWriter.LogWrite("Send receipt to controlunit failed ");
            }
            if (receiptGenerated)
            {

                using (var uof = new UnitOfWork(new ApplicationDbContext()))
                {
                    var ordRepo = uof.OrderRepository;
                    var _order = ordRepo.FirstOrDefault(o => o.Id == orderId);
                    _order.InvoiceDate = DateTime.Now;
                    _order.InvoiceGenerated = 1;
                    if (_order.Status != OrderStatus.ReturnOrder)
                        _order.Status = OrderStatus.Completed;
                    _order.Updated = 1;
                    _order.CheckOutUserId = checkOutUserId;
                    ordRepo.AddOrUpdate(_order);
                    uof.Commit();
                }
                var receiptRepo = new ReceiptHandler();
                receiptRepo.Update(receipt);
            }
            /* End control code */
            // return true;
            return receiptGenerated;
        }



        #region Helper
        private Visibility Showhide(int invoicegenerate)
        {
            if (invoicegenerate == 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        private decimal TotalPaid(Guid orderId)
        {
            using (var db = new ApplicationDbContext())
            {

                var payments = db.Payment.Where(p => p.OrderId == orderId).ToList();
                if (payments.Count > 0)
                    return payments.Sum(p => p.PaidAmount);
                return 0;
            }

        }

        public decimal GetGrossTotal(OrderLine orderdetail)
        {
            decimal grossTotal = (((orderdetail.Direction * orderdetail.Quantity) * orderdetail.UnitPrice - (orderdetail.Direction * orderdetail.Quantity) * orderdetail.UnitPrice / 100 * orderdetail.DiscountPercentage)
                          + ((orderdetail.Direction * orderdetail.Quantity) * orderdetail.UnitPrice - (orderdetail.Direction * orderdetail.Quantity) * orderdetail.UnitPrice / 100 * orderdetail.DiscountPercentage)
                          / 100 * orderdetail.TaxPercent);
            return orderdetail.GrossAmount();


        }

        #endregion


        internal string GetPrinterByAssignedTo(string p)
        {

            try
            {
                using (var db = new ApplicationDbContext())
                {


                    var printer = db.Printer.FirstOrDefault();
                    if (printer != null)
                    {
                        return printer.PrinterName;
                    }
                    else
                    {
                        return "Microsoft XPS Document Writer";
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return "Microsoft XPS Document Writer";
            }
        }
        internal int GetLastBongNo(out int dailyBongCounter, bool IsMinus = false, bool IsPlus = false)
        {
            dailyBongCounter = 1;
            try
            {
                int bongCounter = 0;
                using (var db = new ApplicationDbContext())
                {

                    if (Defaults.BongCounter)
                    {

                        var counter = db.BongCounter.FirstOrDefault();
                        if (counter != null)
                        {
                            bongCounter = counter.Counter;
                        }
                        else
                        {
                            var bong = new BongCounter
                            {
                                Counter = 1,
                                BarCounter = 1
                            };
                            db.BongCounter.Add(bong);
                            db.SaveChanges();
                        }
                    }
                    if (Defaults.DailyBongCounter)
                    {
                        var dailyBong = db.BongCounter.FirstOrDefault(c => c.Id == 2);
                        if (dailyBong == null)
                        {
                            var _bong = new BongCounter
                            {
                                Id = 2,
                                Counter = 1,
                                BarCounter = 1
                            };
                            db.BongCounter.Add(_bong);
                            db.SaveChanges();
                        }
                        else
                        {
                            dailyBongCounter = dailyBong.Counter;
                            //if (IsMinus)
                            //    dailyBongCounter = dailyBongCounter - 1;
                            //if(IsPlus)
                            //    dailyBongCounter = dailyBongCounter + 1;
                        }
                    }


                    return bongCounter;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return 1;
            }
        }



        internal void UpdateBongNo(Guid orderId, int dailyBongCounter)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {

                    var bong = db.BongCounter.FirstOrDefault();
                    var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
                    if (bong != null)
                    {
                        if (string.IsNullOrEmpty(order.Bong) || order.Bong == "0")
                        {
                            order.Bong = bong.Counter.ToString();
                            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        }

                        int lastBong = bong.Counter + 1;
                        bong.Counter = lastBong;
                        db.Entry(bong).State = System.Data.Entity.EntityState.Modified;
                    }
                    if (Defaults.DailyBongCounter)
                    {
                        var _bong = db.BongCounter.FirstOrDefault(b => b.Id == 2);
                        if (_bong != null)
                        {
                            if (String.IsNullOrEmpty(order.DailyBong))
                            {
                                order.DailyBong = dailyBongCounter.ToString();
                                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                            }

                            _bong.Counter = dailyBongCounter + 1;
                            db.Entry(_bong).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        internal void UpdateBongNoForFoodOrder(int dailyBongCounter)
        { 
            try
            {
                using (var db = new ApplicationDbContext())
                {

                    var bong = db.BongCounter.FirstOrDefault();
                    if (bong != null)
                    {
                        int lastBong = bong.Counter + 1;
                        bong.Counter = lastBong;
                        db.Entry(bong).State = System.Data.Entity.EntityState.Modified;
                    }
                    if (Defaults.DailyBongCounter)
                    {
                        var _bong = db.BongCounter.FirstOrDefault(b => b.Id == 2);
                        if (_bong != null)
                        {
                            _bong.Counter = dailyBongCounter + 1;
                            db.Entry(_bong).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }




        internal void UpdateOrderCustomerInfo(Guid orderId, Customer customer, string orderComments)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        order.CustomerId = customer.Id;
                        order.OrderComments = orderComments;
                        if (order.Status != OrderStatus.ReturnOrder)
                            order.Status = OrderStatus.Completed;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        internal decimal UpdateOrderDiscount(Guid orderId, decimal amount, bool percentage)
        {
            decimal orderTotal = 0;
            try
            {

                using (var uof = new UnitOfWork(PosState.GetInstance().Context))
                {
                    var orderRepo = uof.OrderRepository;
                    var orderLineRepo = uof.OrderLineRepository;

                    var order = orderRepo.FirstOrDefault(o => o.Id == orderId);

                    if (order != null)
                    {
                        var lines = orderLineRepo.Where(ol => ol.OrderId == orderId && ol.ItemType == 0 && ol.Product.DiscountAllowed && ol.Active == 1).ToList();
                        var total = lines.Sum(s => s.GrossAmount());

                        // var pp= amount / total * 100;
                        decimal percent = 0;
                        if (percentage)
                        {
                            var totaldiscount = (order.OrderTotal / 100) * amount;
                            percent = totaldiscount / total * 100;
                        }
                        else
                            percent = amount / total * 100;
                        var orderDetails = orderLineRepo.Where(ol => ol.OrderId == orderId && ol.ItemType != ItemType.Grouped && ol.Active == 1).ToList();
                        foreach (var orderDetail in lines)
                        {
                            var grossTotal = orderDetail.GrossAmount();
                            orderDetail.ItemDiscount = (grossTotal / 100) * percent;

                        }
                        orderTotal = orderDetails.Sum(ol => ol.GrossAmountDiscounted());

                        order.OrderTotal = orderTotal;
                        orderRepo.AddOrUpdate(order);
                        uof.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
            return orderTotal;
        }


        internal decimal CancelOrderDiscount(Guid orderId)
        {
            decimal orderTotal = 0;
            try
            {

                using (var db = new ApplicationDbContext())
                {

                    var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);

                    if (order != null)
                    {
                        var lines = db.OrderDetail.Where(ol => ol.OrderId == orderId).ToList();

                        foreach (var orderDetail in lines)
                        {
                            orderDetail.ItemDiscount = 0;
                            orderDetail.DiscountPercentage = 0;
                            db.Entry(orderDetail).State = System.Data.Entity.EntityState.Modified;
                        }
                        orderTotal = lines.Sum(ol => ol.GrossAmountDiscounted());
                        //ucCart.SelectedItem.DiscountPercentage = discountWindow.Amount;
                        //itemdiscount = (grossTotal / 100) * ucCart.SelectedItem.DiscountPercentage;
                        //itemdiscount = qunatity * itemdiscount;
                        order.OrderTotal = orderTotal;
                        db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
            return orderTotal;
        }






        public bool SaveMqttOrder(Order order)
        {

            try
            {

                using (var uof = new UnitOfWork(new ApplicationDbContext()))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
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
                    if (lines != null && lines.Count > 0)
                        foreach (var orderLine in lines)
                        {
                            orderLine.Product = null;
                            orderLineRepo.AddOrUpdate(orderLine);
                        }
                    if (order.Payments != null && order.Payments.Count > 0)
                        foreach (var payment in order.Payments)
                        {
                            paymentRepo.AddOrUpdate(payment);

                        }
                    if (order.Receipts != null && order.Receipts.Count > 0)
                        foreach (var recipt in order.Receipts)
                        {
                            receiptRepo.AddOrUpdate(recipt);

                        }
                    uof.Commit();

                }


                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
                return false;

            }




        }
    }
}