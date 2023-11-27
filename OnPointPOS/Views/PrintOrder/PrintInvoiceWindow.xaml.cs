using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Handlers;
using POSSUM.Res;
using POSSUM.Data;
using System.Configuration;
using System.Windows.Threading;

namespace POSSUM.Views.PrintOrder
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintInvoiceWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        Order orderMaster;
        //   List<VAT> vatAmounts;
        Receipt receipt;
        Guid OrderId = default(Guid);
        private bool marchantPrint = false;
        public string marchantReceipt { get; set; }
        public string customerReceipt { get; set; }
        decimal grandTotal = 0;
        public PrintInvoiceWindow()
        {
            InitializeComponent();

            // gdMain.DataContext = Defaults.CompanyInfo;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            GetOrderById(OrderId, false);
            Dispatcher.BeginInvoke((Action)(() =>
            {

                if (marchantPrint)
                {
                    txtDetail.Dispatcher.BeginInvoke(new Action(() => txtDetail.Text = marchantReceipt));
                    SendToPrint(false);
                    OpenCashDrawer(grandTotal);
                }
                txtDetail.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtDetail.Text = customerReceipt;
                    Dispatcher.BeginInvoke((Action)(() =>
                    { SendToPrint(false); }));
                }));


            }));

            // run all background tasks here
        }
        internal void OpenCashDrawer(decimal amount)
        {
            try
            {

                using (var db = new ApplicationDbContext())
                {

                    var cashdrawer = db.CashDrawer.First(x => x.TerminalId == Defaults.Terminal.Id);

                    // Log that drawer was opened
                    cashdrawer.OpenCashDrawerSale(amount, Defaults.User.Id);

                    // Kick drawer
                    PosState.GetInstance().CashDrawer.Open();

                    // Save log
                    if (cashdrawer.Logs != null && cashdrawer.Logs.Count > 0)
                    {
                        db.CashDrawerLog.AddRange(cashdrawer.Logs);
                    }
                    db.SaveChanges();
                }
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                App.MainWindow.ShowError("Error", "No cashdrawer defined.");
            }
        }

        public PrintInvoiceWindow(Guid orderId, bool isCopy)
        {
            //    Utility.Printing prt = new Utility.Printing();

            //    prt.print();

            InitializeComponent();
            // gdMain.DataContext = Defaults.CompanyInfo;
            OrderId = orderId;
            GetOrderById(orderId, isCopy);
            RenderGridSize();
            // this.Height = gdMain.ActualHeight + 50;
        }
        bool IsReturnInvoice = false;

        public int FindItem(List<string> haystack, string needle)
        {
            for (int i = 0; i < haystack.Count; i++)
                if (haystack[i] == needle) return i;
            return -1;
        }
        public void GetOrderById(Guid orderId, bool isCopy)
        {
            try
            {


                var orderRepo = new OrderRepository(PosState.GetInstance().Context);
                orderMaster = orderRepo.GetOrderMasterDetailById(orderId);
                var depositHistoryForOrder = orderRepo.GetDepositHistoryForOrder(orderId);
                var printModel = new PrintModel
                {

                    Footer = Defaults.Outlet.FooterText,
                    Header = Defaults.Outlet.HeaderText,
                    CompanyInfo = Defaults.CompanyInfo,
                    TaxDesc = Defaults.Outlet.TaxDescription
                };
                printModel.OrderMaster = orderMaster;
                if (File.Exists(Defaults.CompanyInfo.Logo))
                {
                    var uriSource = new Uri(Defaults.CompanyInfo.Logo);

                    logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Source = new BitmapImage(uriSource)));

                }
                else
                {
                    logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Visibility = Visibility.Collapsed));
                    lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Text = string.IsNullOrEmpty(Defaults.Outlet.HeaderText) ? Defaults.Outlet.Name : Defaults.Outlet.HeaderText));
                    lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Visibility = Visibility.Visible));
                }
                txtAddress.Dispatcher.BeginInvoke(new Action(() => txtAddress.Text = Defaults.Outlet.Address));
                txtPhoneno.Dispatcher.BeginInvoke(new Action(() => txtPhoneno.Text = Defaults.Outlet.Phone));
                txtURL.Dispatcher.BeginInvoke(new Action(() => txtURL.Text = Defaults.Outlet.WebUrl));

                txtOrgNo.Dispatcher.BeginInvoke(new Action(() => txtOrgNo.Text = Defaults.Outlet.OrgNo));
                txtOrderDate.Dispatcher.BeginInvoke(new Action(() => txtOrderDate.Text = Convert.ToDateTime(orderMaster.InvoiceDate).ToString("yyyy-MM-dd HH:mm:ss")));
                //  lblTaxDesc.Dispatcher.BeginInvoke(new Action(() => lblTaxDesc.Text = Defaults.Outlet.TaxDescription));
                //lblBong.Text = "Bong#: " + orderMaster.Bong;

                // vatAmounts = new List<VAT>();
                lblInvoiceType.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        if (isCopy)
                            lblInvoiceType.Text = UI.Report_ReceiptCopy;// "KOPIA";                    
                                                                        // Collapse if string is empty
                        lblInvoiceType.Visibility = String.IsNullOrEmpty(lblInvoiceType.Text) ? Visibility.Collapsed : Visibility.Visible;
                    }));
                lblMode.Dispatcher.BeginInvoke(
                   new Action(() =>
                   {

                       if (Defaults.User.TrainingMode)
                       {
                           lblMode.Text = UI.Report_Trainingmode; //"Övning";
                       }
                       // Collapse if string is empty
                       lblMode.Visibility = String.IsNullOrEmpty(lblMode.Text) ? Visibility.Collapsed : Visibility.Visible;
                   }));
                if (orderMaster.Type == OrderType.TakeAway)
                {
                    lblOrderType.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        lblOrderType.Text = UI.Sales_TakeAwayButton;// "TakeAway";                    
                                                                    // Collapse if string is empty
                        lblOrderType.Visibility = Visibility.Visible;
                    }));
                }
                if (orderMaster.Type == OrderType.Return)
                {
                    lblOrderType.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        lblOrderType.Text = UI.Global_Return;// "Return";                    
                                                             // Collapse if string is empty
                        lblOrderType.Visibility = Visibility.Visible;
                    }));
                }

                //customer info
                if (orderMaster.CustomerId != null && orderMaster.CustomerId != default(Guid))
                {
                    InvoiceHandler ordRepo = new InvoiceHandler();
                    var customer = ordRepo.GetCustomer(orderMaster.CustomerId);
                    customerGrid.Visibility = Visibility.Visible;
                    txtCustomerName.Dispatcher.BeginInvoke(new Action(() => { txtCustomerName.Text = customer.Name; txtCustomerName.Visibility = Visibility.Visible; }));
                    txtCustomerAddress.Dispatcher.BeginInvoke(new Action(() => { txtCustomerAddress.Text = customer.Address1; txtCustomerAddress.Visibility = Visibility.Visible; }));
                    string cityZip = customer.City ?? " " + ", " + customer.ZipCode ?? " ";
                    txtCustomerCityZip.Dispatcher.BeginInvoke(new Action(() => { txtCustomerCityZip.Text = cityZip; txtCustomerCityZip.Visibility = Visibility.Visible; }));
                    txtCustomerPhone.Dispatcher.BeginInvoke(new Action(() => { txtCustomerPhone.Text = customer.Phone; txtCustomerPhone.Visibility = Visibility.Visible; }));
                    txtCustomerFloor.Dispatcher.BeginInvoke(new Action(() => { txtCustomerFloor.Text = UI.Global_Floor + ": " + customer.FloorNo; txtCustomerFloor.Visibility = Visibility.Visible; }));
                    txtCustomerPortCode.Dispatcher.BeginInvoke(new Action(() => { txtCustomerPortCode.Text = UI.Global_PortCode + ": " + customer.PortCode; txtCustomerPortCode.Visibility = Visibility.Visible; }));


                }

                receipt = orderMaster.Receipt;
                string comment = "";
                if (!string.IsNullOrEmpty(orderMaster.Comments))
                {
                    comment = orderMaster.Comments;
                }
                else if (!string.IsNullOrEmpty(orderMaster.OrderComments))
                {
                    comment = orderMaster.OrderComments;
                }

                KvittoText.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Defaults.ShowComments)
                    {
                        KvittoText.Text = comment;
                        KvittoText.Visibility = Visibility.Visible;
                        lblKvittoText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        KvittoText.Text = "";
                        KvittoText.Visibility = Visibility.Collapsed;
                        lblKvittoText.Visibility = Visibility.Collapsed;
                    }
                }
                ));

                if (Defaults.SignatureOnReturnReceipt && orderMaster.OrderType == OrderType.Return)
                {
                    lblNamn.Visibility = Visibility.Visible;
                    lblMobnr.Visibility = Visibility.Visible;
                    lblSign.Visibility = Visibility.Visible;
                    lblNamn.MinHeight = 50;
                    lblMobnr.MinHeight = 50;
                    lblSign.MinHeight = 20;
                }

                if (receipt != null)
                {
                    printModel.ReceiptNo = receipt.ReceiptNumber.ToString();
                    printModel.ControlUnitCode = receipt.ControlUnitCode;
                    printModel.ControlUnitName = receipt.ControlUnitName;
                    txtInvoiceNumber.Dispatcher.BeginInvoke(new Action(() => txtInvoiceNumber.Text = receipt.ReceiptNumber.ToString()));



                    ControlUnitName.Dispatcher.BeginInvoke(new Action(() => ControlUnitName.Text = receipt.ControlUnitName));
                    ControlUnitCode.Dispatcher.BeginInvoke(new Action(() => ControlUnitCode.Text = receipt.ControlUnitCode));
                    if (receipt.IsSignature)
                    {
                        marchantPrint = true;
                        marchantReceipt = receipt.MerchantPaymentReceipt;
                        customerReceipt = receipt.CustomerPaymentReceipt;
                        //  printModel.CustomerPaymentReceipt = receipt.MerchantPaymentReceipt;

                        //creditcardPaymentResult.CustomerReceipt.RemoveAt();
                        //FindItem(receipt.CustomerPaymentReceipt, "SIGN:");

                        txtDetail.Dispatcher.BeginInvoke(new Action(() => txtDetail.Text = receipt.CustomerPaymentReceipt));
                    }
                    else if (receipt.CustomerPaymentReceipt != null)
                    {
                        customerReceipt = receipt.CustomerPaymentReceipt;
                        printModel.CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;
                        txtDetail.Dispatcher.BeginInvoke(new Action(() => txtDetail.Text = receipt.CustomerPaymentReceipt));
                    }
                }
                // txtOrderDate.Dispatcher.BeginInvoke(new Action(() => txtOrderDate.Text = Convert.ToDateTime(orderMaster.InvoiceDate).ToShortDateString() + "  " + Convert.ToDateTime(orderMaster.InvoiceDate).ToShortTimeString()));
                printModel.ReceiptDate = DateTime.Now;
                printModel.Cashier = Defaults.User.UserName;
                txtOrderBy.Dispatcher.BeginInvoke(new Action(() => txtOrderBy.Text = Defaults.User.UserName));
                var orderDetails = orderMaster.OrderLines.ToList();// IsReturnInvoice ? orderRepo.GetOrderDetailForReturnInvoiceById(orderId) : orderRepo.GetOrderDetailForInvoiceById(orderId);

                printModel.Items = orderDetails;

                var vatGroups = orderDetails.Where(i => i.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Individuals).GroupBy(od => od.TaxPercent);
                var vatDetails = new List<VATModel>();
                var vatAmounts = new List<VAT>();
                foreach (var grp in vatGroups)
                {
                    decimal vat = grp.First().TaxPercent;
                    decimal total = grp.Sum(tot => tot.GrossAmountDiscounted());
                    if (orderMaster.Type == OrderType.Return)
                        total = grp.Sum(tot => tot.ReturnGrossAmountDiscounted());
                    decimal net = grp.Sum(tot => tot.NetAmount());
                    if (orderMaster.Type == OrderType.Return)
                        net = grp.Sum(tot => tot.ReturnNetAmount());
                    decimal vatAmount = grp.Sum(tot => tot.VatAmount());
                    if (orderMaster.Type == OrderType.Return)
                        vatAmount = grp.Sum(tot => tot.ReturnVatAmount());
                    var vatModel = new VATModel(vat, vatAmount)
                    {
                        NetAmount = net,
                        Total = total
                    };
                    vatDetails.Add(vatModel);
                    vatAmounts.Add(vatModel.GetVatAmounts());
                }

                var ingrideintItems = new List<OrderLine>();
                foreach (var ingrItem in orderDetails)
                {
                    if (ingrItem.IngredientItems != null && ingrItem.IngredientItems.Count() > 0)
                    {
                        ingrideintItems.AddRange(ingrItem.IngredientItems.ToList());
                    }
                }
                if (ingrideintItems.Count > 0)
                {

                    var ingredientVatGroups = ingrideintItems.GroupBy(od => od.TaxPercent);
                    foreach (var ingrp in ingredientVatGroups)
                    {
                        var _itm = ingrp.First();
                        decimal invat = _itm.TaxPercent;
                        decimal intotal = ingrp.Sum(tot => tot.GrossAmountDiscounted());
                        if (orderMaster.Type == OrderType.Return)
                            intotal = ingrp.Sum(tot => tot.ReturnGrossAmountDiscounted());
                        decimal innet = ingrp.Sum(tot => tot.NetAmount());
                        if (orderMaster.Type == OrderType.Return)
                            innet = ingrp.Sum(tot => tot.ReturnNetAmount());
                        decimal invatAmount = ingrp.Sum(tot => tot.VatAmount());
                        if (orderMaster.Type == OrderType.Return)
                            invatAmount = ingrp.Sum(tot => tot.ReturnVatAmount());
                        var _vatModel = new VATModel(invat, invatAmount)
                        {
                            NetAmount = innet,
                            Total = intotal
                        };
                        vatDetails.Add(_vatModel);
                        vatAmounts.Add(_vatModel.GetVatAmounts());
                    }

                }

                foreach (var detail in orderDetails.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
                {
                    var vatInnerGroups = detail.ItemDetails.GroupBy(od => od.TaxPercent);
                    foreach (var grp in vatInnerGroups)
                    {
                        decimal vat = grp.First().TaxPercent;
                        decimal total = grp.Sum(tot => tot.GrossAmountDiscounted());
                        if (orderMaster.Type == OrderType.Return)
                            total = grp.Sum(tot => tot.ReturnGrossAmountDiscounted());
                        decimal vatAmount = grp.Sum(tot => tot.VatAmount());
                        if (orderMaster.Type == OrderType.Return)
                            vatAmount = grp.Sum(tot => tot.ReturnVatAmount());
                        decimal net = grp.Sum(tot => tot.NetAmount());
                        if (orderMaster.Type == OrderType.Return)
                            net = grp.Sum(tot => tot.ReturnNetAmount());
                        var vatModel = new VATModel(vat, vatAmount)
                        {
                            NetAmount = net,
                            Total = total
                        };
                        vatDetails.Add(vatModel);
                        vatAmounts.Add(vatModel.GetVatAmounts());
                    }
                }
                var vatDetailsGroup = vatDetails.GroupBy(o => o.VATPercent).ToList();
                var vat_Details = new List<VATModel>();
                foreach (var vatgrp in vatDetailsGroup)
                {
                    decimal vat = vatgrp.First().VATPercent;
                    decimal total = vatgrp.Sum(tot => tot.Total);
                    decimal net = vatgrp.Sum(tot => tot.NetAmount);
                    decimal vatAmount = vatgrp.Sum(tot => tot.VATTotal);
                    var vatModel = new VATModel(vat, vatAmount)
                    {
                        NetAmount = net,
                        Total = total
                    };

                    vat_Details.Add(vatModel);

                }

                printModel.VatDetails = vat_Details.OrderBy(o => o.VATPercent).ToList();
                printModel.VATAmounts = vatAmounts;
                // printModel.VatDetails = vatDetails.OrderBy(o => o.VATPercent).ToList();
                LBVats.Dispatcher.BeginInvoke(new Action(() => LBVats.ItemsSource = printModel.VatDetails));

                var lstPayments = orderMaster.Payments.ToList();// orderRepo.GetPaymentDetailByOrder(orderId);
                var payments = new List<Payment>();
                foreach (var payment in lstPayments)
                {
                    payments.Add(new Payment
                    {
                        Id = payment.Id,
                        TipAmount = payment.TipAmount,
                        PaidAmount = payment.CashCollected > 0 ? payment.CashCollected : payment.PaidAmount,
                        PaymentRef = payment.PaymentRef,
                        TypeId = payment.TypeId,
                        TypeName = payment.TypeName,
                        CashChange = payment.CashChange,
                        PaymentDate = payment.PaymentDate,
                        CashCollected = payment.CashCollected,
                        CreditCardNo = payment.CreditCardNo,
                        Direction = payment.Direction,
                        ReturnAmount = payment.ReturnAmount
                    });
                }
                printModel.Payments = payments;
                printModel.Tip = Math.Round(payments.Sum(a => a.TipAmount), 2);// grossTotal;// - itemsDiscount;
                lvPayments.Dispatcher.BeginInvoke(new Action(() => lvPayments.ItemsSource = payments.Where(pmt => pmt.PaidAmount != 0).ToList()));
                //decimal freecopon = lstPayments.Where(pmt => pmt.TypeId == 0).Sum(tot => tot.PaidAmount);

                //decimal returncash = lstPayments.Where(pmt => pmt.TypeId == 1 || pmt.TypeId == 7 || pmt.TypeId == 4 || pmt.TypeId == 9 || pmt.TypeId == 10 || pmt.TypeId == 11).Sum(tot => tot.CashChange);

                decimal returncash = lstPayments.Where(pmt => UtilityConstants.ListCashBack.Contains(pmt.TypeId)).Sum(tot => tot.ReturnAmount);

                //decimal returncash = lstPayments.Where(pmt => pmt.TypeId == (int)PaymentTypesEnum.PaidbyCash 
                //|| pmt.TypeId == (int)PaymentTypesEnum.PaidByCreditCard
                //|| pmt.TypeId == (int)PaymentTypesEnum.MobileCard
                //|| pmt.TypeId == (int)PaymentTypesEnum.ElveCard).Sum(tot => tot.ReturnAmount);

                decimal collectedcash = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.CashCollected);
                decimal collectedcard = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.CashCollected);
                decimal tip = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.TipAmount);
                decimal cardtip = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.TipAmount);

                if (orderMaster.Type == OrderType.Return)
                    lblTotalBill.Dispatcher.BeginInvoke(new Action(() => lblTotalBill.Text = UI.CheckOutOrder_Label_TotalReturnBill));
                if (orderMaster.RoundedAmount != 0)
                {
                    printModel.RoundedAmount = orderMaster.RoundedAmount;
                    //txtRoundAmount.Dispatcher.BeginInvoke(new Action(() => txtRoundAmount.Text = orderMaster.RoundedAmount.ToString()));                
                    lblRoundAmount.Dispatcher.BeginInvoke(new Action(() => lblRoundAmount.Visibility = Visibility.Visible));
                    txtRoundAmount.Dispatcher.BeginInvoke(new Action(() => txtRoundAmount.Visibility = Visibility.Visible));
                }

                if (depositHistoryForOrder != null)
                {
                    printModel.NewBalance = depositHistoryForOrder.NewBalance;
                    //txtRoundAmount.Dispatcher.BeginInvoke(new Action(() => txtRoundAmount.Text = orderMaster.RoundedAmount.ToString()));                
                    lblOldBalance.Dispatcher.BeginInvoke(new Action(() => lblNewBalance.Visibility = Visibility.Visible));
                    txtOldBalance.Dispatcher.BeginInvoke(new Action(() => txtNewBalance.Visibility = Visibility.Visible));

                    printModel.OldBalance = depositHistoryForOrder.OldBalance;
                    //txtRoundAmount.Dispatcher.BeginInvoke(new Action(() => txtRoundAmount.Text = orderMaster.RoundedAmount.ToString()));                
                    lblOldBalance.Dispatcher.BeginInvoke(new Action(() => lblOldBalance.Visibility = Visibility.Visible));
                    txtOldBalance.Dispatcher.BeginInvoke(new Action(() => txtOldBalance.Visibility = Visibility.Visible));
                }

                if (returncash > 0)
                {
                    returncash = returncash - tip;
                    printModel.CashBack = returncash;
                    lblChange.Dispatcher.BeginInvoke(new Action(() => lblChange.Visibility = Visibility.Visible));
                    txtChange.Dispatcher.BeginInvoke(new Action(() => txtChange.Visibility = Visibility.Visible));
                    // txtChange.Dispatcher.BeginInvoke(new Action(() => txtChange.Text = returncash + ""));
                }
                if (collectedcash > 0 && orderMaster.Type != OrderType.Return)
                {
                    printModel.CollectedCash = collectedcash;
                    //  txtCollectedCash.Dispatcher.BeginInvoke(new Action(() => txtCollectedCash.Text = collectedcash + ""));
                    // txtCollectedCash.Dispatcher.BeginInvoke(new Action(() => txtCollectedCash.Visibility = Visibility.Visible));
                    // lblCashCollected.Dispatcher.BeginInvoke(new Action(() => lblCashCollected.Visibility = Visibility.Visible));
                }
                if (tip > 0)
                {
                    //lblTip.Dispatcher.BeginInvoke(new Action(() => lblTip.Text = "Extras:"));
                    //printModel.TipAmount = tip;
                    //  txtTipAmount.Dispatcher.BeginInvoke(new Action(() => txtTipAmount.Text =  tip.ToString("C")));
                }
                if (collectedcard > 0 && orderMaster.Type != OrderType.Return)
                {
                    // lblCollectCard.Dispatcher.BeginInvoke(new Action(() => lblCollectCard.Visibility = Visibility.Visible));
                    //  txtCollectCard.Dispatcher.BeginInvoke(new Action(() => txtCollectCard.Visibility = Visibility.Visible));
                    printModel.CollectCard = collectedcard;
                    //  txtCollectCard.Dispatcher.BeginInvoke(new Action(() => txtCollectCard.Text = collectedcard + ""));
                }


                //decimal grossTotal = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
                //if (orderMaster.Type == OrderType.Return)
                //    grossTotal = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ReturnGrossAmountDiscounted()), 2);

                decimal grossTotal = Math.Round(orderMaster.OrderTotal, 2);
                decimal ingrTotal = Math.Round(ingrideintItems.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
                if (orderMaster.Type == OrderType.Return)
                    ingrTotal = Math.Round(ingrideintItems.Where(itm => itm.IsValid).Sum(tot => tot.ReturnGrossAmountDiscounted()), 2);
                var totalVat = orderDetails.Sum(ol => ol.VatAmount());
                if (orderMaster.Type == OrderType.Return)
                    totalVat = orderDetails.Sum(ol => ol.ReturnVatAmount());
                var _ingrVat = ingrideintItems.Sum(ol => ol.VatAmount());
                if (orderMaster.Type == OrderType.Return)
                    _ingrVat = ingrideintItems.Sum(ol => ol.ReturnVatAmount());
                totalVat = totalVat + _ingrVat;
                grossTotal = grossTotal + ingrTotal;
                decimal netTotal = grossTotal - totalVat;
                printModel.GrandTotal = Math.Round(orderMaster.OrderTotal, 2);// grossTotal;// - itemsDiscount;
                printModel.NetTotal = netTotal;
                grandTotal = grossTotal;
                //  txtEndTotal.Dispatcher.BeginInvoke(new Action(() => txtEndTotal.Text =  grossTotal.ToString("C")));
                printModel.Items = orderDetails.Where(itm => itm.IsValid).ToList();

                printModel.Discounts = new List<DiscountsViewModel>()
                {
                    new DiscountsViewModel()
                    {
                        Name = "Total rabatt :",
                        Value = orderDetails.Where(a=>a.ItemDiscount != 0).Sum(a=>a.ItemDiscount)
                    }
                };

                printModel.HasDiscounts = orderDetails.Where(a => a.ItemDiscount > 0).Sum(a => a.ItemDiscount) != 0;

                LBItems.Dispatcher.BeginInvoke(new Action(() => LBItems.ItemsSource = orderDetails.Where(itm => itm.IsValid).ToList()));

                if (printModel.Tip > 0)
                {
                    lblTip.Dispatcher.BeginInvoke(new Action(() => lblTip.Visibility = Visibility.Visible));
                    lblNetPayable.Dispatcher.BeginInvoke(new Action(() => lblNetPayable.Visibility = Visibility.Visible));
                    txtTip.Dispatcher.BeginInvoke(new Action(() => txtTip.Visibility = Visibility.Visible));
                    txtNetPayable.Dispatcher.BeginInvoke(new Action(() => txtNetPayable.Visibility = Visibility.Visible));
                }

                while (vatAmounts.Count < 4)
                {
                    vatAmounts.Add(new VAT(0, 0));
                }
                receipt.VatDetails = vatAmounts;

                if (isCopy)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => this.DataContext = printModel));
                    // TODO, terminal id is int
                    //ReceiptRepository receiptRepo = new ReceiptRepository();
                    //Receipt receipt = receiptRepo.GetByOrderId(orderMaster.OrderId);

                    // Dont allow printing if more then 1 copy already was printed
                    if (receipt.ReceiptCopies >= 1)
                    {
                        btnPrint.Visibility = System.Windows.Visibility.Collapsed;
                        return;
                    }
                    else
                    {
                        btnPrint.Visibility = System.Windows.Visibility.Visible;
                    }
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.DataContext = printModel;

                        if (marchantPrint)
                        {
                            txtDetail.Dispatcher.BeginInvoke(new Action(() => txtDetail.Text = marchantReceipt));
                            SendToPrint(false);
                            OpenCashDrawer(grandTotal);
                        }
                        txtDetail.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            txtDetail.Text = customerReceipt;
                            Dispatcher.BeginInvoke((Action)(() =>
                            { SendToPrint(false); }));
                        }));
                    }));
                }

                // 
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void setReceiptVatAmount(List<VAT> vatAmounts, decimal vatPercent, List<OrderLine> orderDetails, TextBlock vatPercentTextBlock, TextBlock vatNetTextBlock, TextBlock vatTotalTextBlock)
        {
            var TaxPerDetails = orderDetails.Where(t => t.TaxPercent == vatPercent).ToList();
            if (TaxPerDetails.Count > 0)
            {
                decimal Tot = TaxPerDetails.Sum(tot => tot.GrossAmount());
                decimal net = TaxPerDetails.Sum(tot => tot.NetAmount());
                decimal vat = TaxPerDetails.Sum(tot => tot.VatAmount());
                vatPercentTextBlock.Dispatcher.BeginInvoke(new Action(() => vatPercentTextBlock.Text = vat.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
                vatNetTextBlock.Dispatcher.BeginInvoke(new Action(() => vatNetTextBlock.Text = net.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
                vatTotalTextBlock.Dispatcher.BeginInvoke(new Action(() => vatTotalTextBlock.Text = Tot.ToString("C", Defaults.UICultureInfoWithoutCurrencySymbol)));
                vatAmounts.Add(new VAT(vatPercent, vat));
            }
        }

        public void PrintInvoice(Guid orderId, bool isCopy)
        {
            //Utility.Printing prt = new Utility.Printing();
            //prt.print();
            OrderId = orderId;
            GetOrderById(orderId, false);

            //worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            //worker.RunWorkerAsync();

        }
        public void PrintReturnInvoice(Guid orderId)
        {
            lblInvoiceType.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    lblInvoiceType.Text = UI.Sales_ReturnOrder;// "ÅTERKÖP";
                    // Collapse if string is empty
                    lblInvoiceType.Visibility = String.IsNullOrEmpty(lblInvoiceType.Text) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }));
            IsReturnInvoice = true;
            OrderId = orderId;
            GetOrderById(orderId, false);
            //worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            //worker.RunWorkerAsync();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                e.Handled = true;
                DialogResult = false;
                this.Close();
            }));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                LogWriter.JournalLog(Defaults.User.TrainingMode ? Convert.ToInt16(JournalActionCode.ReceiptCopyPrintedViaTrainingMode) : Convert.ToInt16(JournalActionCode.ReceiptCopyPrinted), orderMaster.Id);
                DirectPrint directPrint = new DirectPrint();
                directPrint.PrintReceipt(OrderId, true, true);
                this.DialogResult = true;
                //  SendToPrint(true);
            }));
            
        }

        public void SendToPrint(bool isCopy)
        {
            try
            {

                if (isCopy)
                {
                    ReceiptHandler receiptRepo = new ReceiptHandler();
                    //Receipt receipt = receiptRepo.GetByOrderId(orderMaster.Id);
                    /* Start control code */
                    using (var cuAction = PosState.GetInstance().ControlUnitAction)
                    {
                        cuAction.ControlUnit.RegisterPOS(Defaults.CompanyInfo.OrgNo, Defaults.Terminal.TerminalNo.ToString());
                        int attempts = 1;
                        var x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, 1, true); // true for Clean cash debugging
                        while (x == null && attempts < 4)
                        {
                            attempts++;
                            x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, attempts, true); // true for Clean cash debugging
                        }

                        // Assigned a dummy code and unit name to avoid removing this from database
                        if (x == null)
                        {
                            x = new ControlUnitResponse(true, "CONTROL_UNIT_FAILED", "1234567890abcdefghijklmnopqrstuvwxyz");
                        }
                        receipt.ReceiptCopies++;


                        
                        ControlUnitName.Dispatcher.BeginInvoke(new Action(() => ControlUnitName.Text = x.Success ? x.UnitName : "Unable to connect to controlunit"));
                        ControlUnitCode.Dispatcher.BeginInvoke(new Action(() => ControlUnitCode.Text = x.Success ? x.ControlCode : "Unable to connect to controlunit"));
                    }
                    receiptRepo.Update(receipt);
                    /* End control code */
                    txtInvoiceNumber.Dispatcher.BeginInvoke(new Action(() => txtInvoiceNumber.Text = receipt.ReceiptNumber + " (" + UI.Report_ReceiptCopy + ")"));

                }


                var dlg = new PrintDialog();

                // GM:TODO
              
                string printer = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                // paginator.PageSize = new Size(350, 550 + itemheight + pheight);
                paginator.PageSize = new Size(330, gdMain.ActualHeight + 200);

                if (string.IsNullOrEmpty(printer))
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(paginator, "Order Print");
                        this.Close();
                    }
                }
                else
                {

                    PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                    PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                    var data = printers.Where(pn => pn.Name == printer);
                    if (data.Count() > 0)
                    {
                        prntque = printers.Single(pn => pn.Name == printer);
                    }
                    else // Fall back to XPS Document Writer
                    {
                        printer = "Microsoft XPS Document Writer";
                        printers = new PrintServer().GetPrintQueues();
                        prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                        data = printers.Where(pn => pn.Name == printer);
                        if (data.Count() > 0)
                        {
                            prntque = printers.Single(pn => pn.Name == printer);
                        }
                    }


                    dlg.PrintQueue = prntque; //


                    // dlg.PrintTicket.CopyCount = 3; // number of copies
                    // dlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
                    //if(isCopy==false)
                    //    LogWriter.JournalLog(Defaults.User.TrainingMode ? Defaults.ActionList[JournalActionCode.ReceiptPrintedViaTrainingMode].Id : Defaults.ActionList[JournalActionCode.ReceiptPrinted].Id, orderMaster.Id);
                    dlg.PrintDocument(paginator, "Print Order");

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(UI.Message_PrintingProblemTryLater, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void RenderGridSize()
        {
            gdMain.Measure(new Size(Double.MaxValue, Double.MaxValue));
            Size visualSize = gdMain.DesiredSize;
            gdMain.Arrange(new Rect(new Point(0, 0), visualSize));
            gdMain.UpdateLayout();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    var dlg = new PrintDialog();
                    string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                    RenderGridSize();
                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new Size(350, gdMain.ActualHeight + 100);
                    PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                    PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();

                    printerName = "Microsoft XPS Document Writer";
                    printers = new PrintServer().GetPrintQueues();
                    prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                    var data = printers.Where(pn => pn.Name == printerName);
                    if (data.Count() > 0)
                    {
                        prntque = printers.Single(pn => pn.Name == printerName);
                    }


                    dlg.PrintQueue = prntque;
                    dlg.PrintDocument(paginator, "Receipt");

                    this.Close();
                }));

                
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                e.Handled = true;
                DialogResult = false;
                this.Close();
            }));
        }

        private void btnPrint_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                LogWriter.JournalLog(Defaults.User.TrainingMode ? Convert.ToInt16(JournalActionCode.ReceiptCopyPrintedViaTrainingMode) : Convert.ToInt16(JournalActionCode.ReceiptCopyPrinted), orderMaster.Id);
                DirectPrint directPrint = new DirectPrint();
                directPrint.PrintReceipt(OrderId, true, true);
                this.DialogResult = true;
                //  SendToPrint(true);
            }));
        }

        private void btnImport_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    var dlg = new PrintDialog();
                    string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                    RenderGridSize();
                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new Size(350, gdMain.ActualHeight + 100);
                    PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                    PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();

                    printerName = "Microsoft XPS Document Writer";
                    printers = new PrintServer().GetPrintQueues();
                    prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                    var data = printers.Where(pn => pn.Name == printerName);
                    if (data.Count() > 0)
                    {
                        prntque = printers.Single(pn => pn.Name == printerName);
                    }


                    dlg.PrintQueue = prntque;
                    dlg.PrintDocument(paginator, "Receipt");

                    this.Close();
                }));


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class DiscountsViewModel
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
}