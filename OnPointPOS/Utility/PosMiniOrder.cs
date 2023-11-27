using POSSUM.Data;
using POSSUM.Handlers;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Pdf;
using POSSUM.Presenters.Sales;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utility
{
    public class PosMiniOrder
    {
        ApplicationDbContext db;
        public Guid OrderId { get; set; }
        Customer selectedCustomer = null;
        public bool ReceiptGenerated = false;

        public Guid InvoiceId { get; set; }
        public PosMiniOrder()
        {
            db = PosState.GetInstance().Context;
        }

        public bool HandleCheckOutClick(Order MasterOrder, decimal accountAmount, decimal tipAmount, string paymentTypeRef, bool printReciept = true)
        {
            try
            {

                Defaults.PerformanceLog.Add("Clicked On Check out Button   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                if (MasterOrder.OrderLines.Count == 0)
                {
                    App.MainWindow.ShowError(Defaults.AppProvider.AppTitle, UI.PlanceOrder_CartEmpty);
                    return false;
                }

                if (Defaults.OrderEntryType == OrderEntryType.RecordAll)
                {
                    MasterOrder.Status = MasterOrder.OrderStatusFromType;
                    MasterOrder.Updated = 1;
                    MasterOrder.CheckOutUserId = Defaults.User.Id;
                    MasterOrder.UserId = Defaults.User.Id;
                    var printList = MasterOrder.OrderLines.Where(i => i.Product.Bong && i.ItemStatus != 3).ToList();
                    UpdateOrderDetail(MasterOrder.OrderLines.ToList(), MasterOrder);

                    var createdOrder = new OrderRepository(PosState.GetInstance().Context).SaveOrderMaster(MasterOrder);

                    selectedCustomer = createdOrder.Customer;
                    OrderId = createdOrder.Id;

                    var statusComplete = CheckOut_Order(createdOrder, accountAmount, tipAmount, paymentTypeRef, printReciept);
                    if (statusComplete)
                    {

                        //i.ItemStatus == (int)OrderStatus.New &&
                        var groupItemList = MasterOrder.OrderLines.Where(i => i.Product.ItemType == ItemType.Grouped).ToList();
                        foreach (var itm in groupItemList)
                        {
                            var lst =
                                itm.ItemDetails.Where(i => i.Product.Bong && i.ItemStatus != 3)
                                    .Select(
                                        i =>
                                            new OrderLine
                                            {
                                                OrderId = MasterOrder.Id,
                                                Quantity = i.Quantity,
                                                UnitPrice = i.UnitPrice,
                                                ItemId = i.Product.Id,
                                                Product = i.Product
                                            })
                                    .ToList();
                            printList.AddRange(lst);
                        }

                        if (printList.Count > 0 && Defaults.SaleType == SaleType.Restaurant && Defaults.BONG && MasterOrder.OrderStatusFromType != OrderStatus.ReturnOrder && printReciept)
                        {
                            var drctOrderPrint = new DirectPrint();
                            var order = new Order
                            {
                                Id = MasterOrder.Id,
                                CreationDate = MasterOrder.CreationDate,
                                Bong = MasterOrder.Bong,
                                DailyBong = MasterOrder.DailyBong,
                                Type = MasterOrder.Type,
                                TableId = MasterOrder.TableId,
                                TableName = MasterOrder.TableName,
                                OrderComments = MasterOrder.OrderComments,
                                OrderLines = printList
                            };

                            drctOrderPrint.PrintBong(order, true);
                        }

                        if (ReceiptGenerated)
                        {
                            Defaults.PerformanceLog.Add("Sending for print invoice");
                            LogWriter.CheckOutLogWrite("Sending receipt for print invoice", MasterOrder.Id);
                            //  var invoiceWindow = new PrintNewInvoiceWindow();
                            var directPrint = new DirectPrint();

                            //directPrint.PrintReceiptForAccountCustomer(MasterOrder.Id, false, ReceiptGenerated);
                            if (Defaults.POSMiniPrintForAccountCustomer == "1" && printReciept)
                            {
                                directPrint.PrintReceipt(MasterOrder.Id, false, ReceiptGenerated);
                            }
                            if (MasterOrder.OrderType == OrderType.Return)
                            {
                                // invoiceWindow.PrintReturnInvoice(orderId);
                                LogWriter.JournalLog(
                                        Defaults.User.TrainingMode
                                            ? JournalActionCode.ReceiptPrintedForReturnOrderViaTrainingMode
                                            : JournalActionCode.ReceiptPrintedForReturnOrder, MasterOrder.Id);
                            }
                            else
                            {
                                //  invoiceWindow.PrintInvoice(orderId, false);
                                LogWriter.JournalLog(
                                        Defaults.User.TrainingMode
                                            ? JournalActionCode.ReceiptPrintedViaTrainingMode
                                            : JournalActionCode.ReceiptPrinted, MasterOrder.Id);
                            }
                        }
                    }

                    if (statusComplete)
                    {
                        //UpdateHistoryGrid();
                    }

                }
                Defaults.PerformanceLog.Add("Checkout starting....         -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }


        List<TipModViewModel> TipModcls = new List<TipModViewModel>();
        public bool CheckOut_Order(Order order, decimal accountAmount, decimal tipAmount, string paymentTypeRef, bool printReciept)
        {
            try
            {
                var orderDirection = order.OrderType == OrderType.Return ? -1 : 1;// _direction;

                var TotalBillAmount = order.OrderTotal < 0 ? (orderDirection) * order.OrderTotal : order.OrderTotal;
                var masterorder = new OrderRepository(PosState.GetInstance().Context).GetOrderMasterDetailById(OrderId);
                try
                {

                    if (Defaults.IsOpenOrder == false && masterorder.OrderStatusFromType != OrderStatus.ReturnOrder && string.IsNullOrEmpty(masterorder.Bong) && printReciept)
                    {
                        if (Defaults.BONG && Defaults.DirectBONG)
                        {
                            LogWriter.LogWrite(new Exception("Defaults.BONG" + OrderId));
                            new DirectPrint().PrintBong(masterorder, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.CheckOutLogWrite("CheckOutOrder_Click exception" + ex.Message.ToString(), OrderId);
                    return false;
                }


                if (CheckControlUnitStatus() == false)
                {
                    LogWriter.CheckOutLogWrite("Checking Control Unit Status->", OrderId);
                    CUConnectionWindow cuConnectionWindow = new CUConnectionWindow();
                    if (cuConnectionWindow.ShowDialog() == false)
                        return false;
                }

                if (masterorder != null && masterorder.OrderDirection > 0 && printReciept)
                {
                    if (selectedCustomer != null && TotalBillAmount > selectedCustomer.DepositAmount && selectedCustomer.HasDeposit == true)
                    {
                        var msg = "Insufficient Deposit Amount!";
                        return false;
                    }
                }

                var seemlesProducts = masterorder.OrderLines.Where(p => p.Product.Seamless == true).ToList();
                if (seemlesProducts.Count > 0)
                {
                    List<Product> products = new List<Product>();
                    foreach (var detail in seemlesProducts)
                    {
                        for (int i = 1; i <= detail.Quantity; i++)
                            products.Add(detail.Product);
                    }


                }

                TipModCalculation(TotalBillAmount, tipAmount, accountAmount);
                bool bRes = false;
                bRes = CheckOut(accountAmount, tipAmount, order.OrderComments, 1, paymentTypeRef);
                // ProgressWindow progressDialog = new ProgressWindow();
                if (bRes && printReciept)
                {
                    try
                    {
                        if (accountAmount > 0 && selectedCustomer.DirectPrint)
                            PrintCustomerInvoice();

                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogWrite(ex);
                    }
                }
                return bRes;

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }



        public bool CheckOut(decimal accountAmount, decimal tipamounts, string OrderComments, int orderDirection = 1, string currentPaymentType = "Swish")
        {
            bool res = false;
            List<TipModViewModel> tipModclass = TipModcls;
            decimal tipamount = tipamounts;
            Payment dtoPayment = new Payment();
            bool hasTipIncluded = false;
            LogWriter.CheckOutLogWrite("Saving payment info", OrderId);

            if (accountAmount > 0)
            {
                decimal amountToReturn = 0;


                if (tipamount > 0 && !hasTipIncluded)
                {
                    accountAmount = accountAmount - tipamount;
                }
                dtoPayment = new Payment
                {
                    OrderId = OrderId,
                    PaidAmount = accountAmount - amountToReturn,
                    PaymentDate = DateTime.Now,
                    TypeId = string.IsNullOrEmpty(currentPaymentType) ? 2 : (currentPaymentType.Equals("Swish") ? 10 : 9),
                    CashCollected = accountAmount,
                    CashChange = 0,
                    ReturnAmount = 0,// amountToReturn,
                    //PaymentRef = UI.CheckOutOrder_Method_Account,// "Faktura",
                    PaymentRef = currentPaymentType,// "Faktura",
                    TipAmount = hasTipIncluded ? 0 : tipamount,
                    Direction = orderDirection
                };
                res = SavePayment(dtoPayment);
                hasTipIncluded = true;
                if (res)
                {
                    UpdateOrderCustomerInfo(OrderId, selectedCustomer, OrderComments);
                }

            }
            try
            {
                PaymentTransactionStatus creditcardPaymentResult = null;
                var RemainingAmount = 0;
                if (RemainingAmount == 00m)
                {
                    try
                    {

                        ReceiptGenerated = GenerateInvoiceLocal(OrderId, App.MainWindow.ShiftNo, Defaults.User.Id, creditcardPaymentResult);
                        if (ReceiptGenerated == false)
                        {
                            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReceiptFail), OrderId);
                        }
                        else
                        {
                            LogWriter.CheckOutLogWrite("Generating receipt process completed successfully", OrderId);
                        }
                    }
                    catch (ControlUnitException e)
                    {
                        LogWriter.CheckOutLogWrite(e.Message, OrderId);
                    }
                    catch (Exception ex)
                    {
                        LogWriter.CheckOutLogWrite(ex.Message, OrderId);
                        LogWriter.LogWrite(ex);
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (ControlUnitException e)
            {
                LogWriter.LogException(e);
            }

            Defaults.PerformanceLog.Add("Checkout completed...         -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
            return false;
        }


        public bool UpdateOrderDetail(List<OrderLine> orderDetail, Order order)
        {
            try
            {
                if (orderDetail.Count == 0)
                    return true;
                List<OrderLine> lines = new List<OrderLine>();
                foreach (var line in orderDetail)
                {

                    line.ItemStatus = (int)order.OrderStatusFromType;
                    lines.Add(line);


                }

                new OrderRepository(PosState.GetInstance().Context).SaveOrderLines(lines, order);

                return true;

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return false;
            }
        }
        public bool GenerateInvoiceLocal(Guid id, int shifNo, string userId, PaymentTransactionStatus creditcardPaymentResult)
        {
            return new InvoiceHandler().GenerateInvoiceLocal(id, shifNo, userId, creditcardPaymentResult);
        }

        public bool IsOrderExistInDepositHistory(Guid orderId)
        {
            try
            {
                return new CustomerRepository().IsOrderExistInDepositHistory(orderId);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            try
            {
                return new CustomerRepository().UpdateCustomer(customer);

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateOrderCustomerInfo(Guid id, Customer customer, string orderComments = "")
        {
            return new OrderRepository(db).UpdateOrderCustomerInfo(id, customer, orderComments);
        }



        public bool SavePayment(Payment payment, decimal roundAmount = 0)
        {

            return new InvoiceHandler().SaveOrderPayment(payment, roundAmount);
        }

        public bool CheckControlUnitStatus()
        {
            try
            {
                var uc = PosState.GetInstance().ControlUnitAction;
                uc.ControlUnit.CheckStatus();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        void TipModCalculation(decimal totalBillAmount, decimal tipAmount, decimal accountAmount)
        {
            try
            {

                decimal freecouponAmount = 0;
                decimal RunningTotal = 0;
                if (tipAmount > 0)
                {
                    TipModcls = new List<TipModViewModel>();
                    RunningTotal = freecouponAmount - totalBillAmount;


                    RunningTotal = accountAmount + RunningTotal;
                    if (RunningTotal > 0 && RunningTotal >= tipAmount)
                    {
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "Account",
                            tipModeAmount = tipAmount
                        });
                        return;
                    }
                    else if (RunningTotal > 0 && accountAmount > 0)
                    {
                        //tipBalance = RunningTotal;
                        TipModcls.Add(new TipModViewModel()
                        {
                            tipMode = "Account",
                            tipModeAmount = RunningTotal
                        });
                        tipAmount = tipAmount - RunningTotal;
                        RunningTotal = RunningTotal - RunningTotal;
                    }


                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void PrintCustomerInvoice()
        {
            try
            {
                OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
                var invoiceId = orderRepo.GenerateCustomerInvoice(OrderId, selectedCustomer.Id);
                MemoryStream ms = GenerateOcrPdf(invoiceId);
                string fileName = Environment.CurrentDirectory + @"\temp.pdf";
                //  string fileName = @"C:\NIMPOS\temp.pdf";
                FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                ms.WriteTo(file);
                file.Close();
                ms.Close();
                string printerName = GetPrinterName();
                PdfPrinterHelper.SendFileToPrinter(fileName, printerName);

            }
            catch (Exception ex)
            {

                // throw;
            }
        }

        private string GetPrinterName()
        {
            string printerName = "Microsoft XPS Document Writer";
            var printer = Defaults.Printers.FirstOrDefault(p => p.LocationName == "Invoice");
            if (printer != null)
                printerName = printer.PrinterName;
            PrintQueueCollection printers = new PrintServer().GetPrintQueues();
            PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
            var data = printers.Where(pn => pn.Name == printerName);
            if (data.Count() > 0)
            {
                prntque = printers.FirstOrDefault(pn => pn.Name == printerName);
                if (prntque == null)
                    printerName = "Microsoft XPS Document Writer";
            }
            else // Fall back to XPS Document Writer
            {

                printerName = "Microsoft XPS Document Writer";
            }

            return printerName;
        }
        private MemoryStream GenerateOcrPdf(Guid invoiceId)
        {
            //var orderRepo = new OrderRepository(PosState.GetInstance().Context);
            //var customerIvoice = orderRepo.GetCustomerInvoice(invoiceId);

            var orderRepo = new InvoiceHandler();
            var customerIvoice = orderRepo.GetCustomerInvoice2(invoiceId);

            var invoice = customerIvoice.Invoice;
            var orderDetails = customerIvoice.OrderDetails;
            var customer = customerIvoice.Customer;
            //Dictionary<string, string> orderLines = new Dictionary<string, string>();
            //foreach (var orderItem in orderDetails)
            //{
            //    orderLines.Add(orderItem.ItemName, Text.FormatNumber(orderItem.UnitPrice, 2));
            //}


            decimal grossTotal = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
            // decimal itemsDiscount = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ItemDiscount), 2);
            var totalVat = orderDetails.Sum(ol => ol.VatAmount());

            decimal netTotal = grossTotal - totalVat;

            decimal decTotal = grossTotal;
            decimal decVATPercent = totalVat;// orderDetails.FirstOrDefault().VAT; // Get first VAT available

            string strReminderText = string.Empty;
            //if (receipt.ReminderFee > 0 && invoice.Reminder)
            //{
            //    strReminderText = string.Concat("Vid sen betalning tar vi ut en påminnelseavgift på ", Text.FormatNumber(invoice.ReminderFee), " kr.");
            //}

            //decimal decReminderFee = 0;
            //if (invoiceType == InvoiceType.Reminder)
            //{
            //    decReminderFee = invoice.ReminderFee;
            //}
            decimal decReminderFee = 0;
            string strOcr = invoice.InvoiceNumber.ToString();
            DateTime dtExpireDate = DateTime.Now.AddDays(Defaults.InvoiceDueDays);// invoice.LastRetryDateTime.Date.AddDays(invoice.Interval);

            return Pdf.Ocr.Create(
                invoice.InvoiceNumber
                , Defaults.Outlet.Name
                , invoice.CreationDate
                , dtExpireDate
                , customer.Name
                , strOcr
                , invoice.InvoiceNumber
                , invoice.Remarks// orderMaster.InvoiceNumber //  string.Concat(invoice.tOrder.tCustomer.tCompany.tCorporate.FirstName, " ", invoice.tOrder.tCustomer.tCompany.tCorporate.LastName)
                , " "
                , orderDetails
                , Defaults.Terminal.UniqueIdentification + "-" + invoice.InvoiceNumber.ToString()
                , "Customer Invoice"
                , decTotal
                , decVATPercent
                , netTotal
                , strReminderText
                , string.Concat(customer.Name, " ", customer.OrgNo)
                , customer.Address1
                , string.Concat(customer.ZipCode, " ", customer.City)
                , dtExpireDate
                , decReminderFee
                );

            // decTotalAmount * companyAgreement.DibsPartnerPercent / 100
        }
    }
}
