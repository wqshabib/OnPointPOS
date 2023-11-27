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

namespace POSSUM.Views.PrintOrder
{
    public partial class PrintBillWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private Guid OrderId = default(Guid);
        private Order orderMaster;

        public PrintBillWindow()
        {
            InitializeComponent();
        }

        public PrintBillWindow(Guid orderId)
        {
            InitializeComponent();
            OrderId = orderId;
            InfoCanvas.DataContext = Defaults.CompanyInfo;
             GetOrderById(orderId);
            SendPrint();
        }

        public void PrintBill(Guid orderId)
        {
            OrderId = orderId;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
            // InfoCanvas.DataContext = Defaults.CompanyInfo;
            //GetOrderById(orderId);
            //Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    SendPrint();
            //}));
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            GetOrderById(OrderId);
            Dispatcher.BeginInvoke((Action) (() => { SendPrint(); }));

            // run all background tasks here
        }

        public void GetOrderById(Guid orderId)
        {
            try
            {
                var orderRepo = new OrderRepository(PosState.GetInstance().Context);
                orderMaster = orderRepo.GetOrderMaster(orderId);
                var payments = new List<Payment>();

                if (orderMaster.Payments != null)
                {
                    var lstPayments = orderMaster.Payments.ToList();// orderRepo.GetPaymentDetailByOrder(orderId);

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
                }

                var Tip = Math.Round(payments.Sum(a => a.TipAmount), 2);// grossTotal;// - itemsDiscount;

                if (File.Exists(Defaults.CompanyInfo.Logo))
                {
                    var uriSource = new Uri(Defaults.CompanyInfo.Logo);
                    logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Source = new BitmapImage(uriSource)));
                }
                else
                {
                    logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Visibility = Visibility.Collapsed));
                    lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Text = Defaults.Outlet.Name));
                    lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Visibility = Visibility.Visible));
                }
                txtOutletName.Dispatcher.BeginInvoke(new Action(() => txtOutletName.Text = Defaults.CompanyInfo.Name));
                txtAddress.Dispatcher.BeginInvoke(new Action(() => txtAddress.Text = Defaults.CompanyInfo.Address));
                txtPhoneno.Dispatcher.BeginInvoke(new Action(() => txtPhoneno.Text = Defaults.CompanyInfo.Phone));
                txtOrgNo.Dispatcher.BeginInvoke(new Action(() => txtOrgNo.Text = Defaults.CompanyInfo.OrgNo));
                txtURL.Dispatcher.BeginInvoke(new Action(() => txtURL.Text = Defaults.CompanyInfo.URL));

                //  lblFooter.Dispatcher.BeginInvoke(new Action(() => lblFooter.Text = Defaults.Outlet.FooterText));

                lblMode.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Defaults.User.TrainingMode)
                    {
                        lblMode.Text = UI.Report_Trainingmode;
                    }
                    // Collapse if string is empty
                    lblMode.Visibility = string.IsNullOrEmpty(lblMode.Text) ? Visibility.Collapsed : Visibility.Visible;
                }));
                txtOrderNumber.Dispatcher.BeginInvoke(new Action(() => txtOrderNumber.Text = orderMaster.OrderNoOfDay));
                txtOrderDate.Dispatcher.BeginInvoke(
                    new Action(
                        () =>
                            txtOrderDate.Text =
                                Convert.ToDateTime(orderMaster.CreationDate).ToShortDateString() + "  " +
                                DateTime.Now.ToShortTimeString()));
                // printModel.ReceiptDate = DateTime.Now;
                // printModel.Cashier = Defaults.User.Id.ToString();
                txtOrderBy.Dispatcher.BeginInvoke(new Action(() => txtOrderBy.Text = Defaults.User.UserName));
                // +" - " + orderMaster.TableName;

                var orderDetails = orderRepo.GetOrderLinesById(orderId);

                //  printModel.Items = orderDetails;
                var vatGroups = orderDetails.GroupBy(od => od.TaxPercent);
                var vatDetails = new List<VATModel>();

                foreach (var grp in vatGroups)
                {
                    decimal _vat = grp.First().TaxPercent;
                    decimal total = grp.Sum(tot => tot.GrossAmount());
                    decimal net = grp.Sum(tot => tot.NetAmount());
                    decimal _vatAmount = grp.Sum(tot => tot.VatAmount());

                    var vatModel = new VATModel(_vat, _vatAmount) { NetAmount = net, Total = total };
                    vatDetails.Add(vatModel);
                }
                //  printModel.VatDetails = vatDetails;
                LBVats.Dispatcher.BeginInvoke(new Action(() => LBVats.ItemsSource = vatDetails));

                var vat0PerGroups = orderDetails.Where(od => od.TaxPercent == 0).ToList();
                decimal vat = 0;
                decimal vatAmount = vat0PerGroups.Sum(tot => tot.VatAmount());
                VAT vat1 = new VAT(vat, vatAmount);
                var vat6PerGroups = orderDetails.Where(od => od.TaxPercent == 6).ToList();
                vat = 6;
                vatAmount = vat6PerGroups.Sum(tot => tot.VatAmount());
                VAT vat2 = new VAT(vat, vatAmount);
                var vat12PerGroups = orderDetails.Where(od => od.TaxPercent == 12).ToList();
                vat = 12;
                vatAmount = vat12PerGroups.Sum(tot => tot.VatAmount());
                VAT vat3 = new VAT(vat, vatAmount);
                var vat25PerGroups = orderDetails.Where(od => od.TaxPercent == 25).ToList();
                vat = 25;
                vatAmount = vat25PerGroups.Sum(tot => tot.VatAmount());
                VAT vat4 = new VAT(vat, vatAmount);

                //using (var cuAction = PosState.GetInstance().ControlUnitAction)
                //{
                //    cuAction.ControlUnit.RegisterPOS(Defaults.CompanyInfo.OrgNo, Defaults.Terminal.TerminalNo.ToString());
                //    ControlUnitResponse x = cuAction.ControlUnit.SendReceipt(orderMaster.CreationDate, 1, orderMaster.OrderTotal,
                //        0, vat1, vat2, vat3, vat4, false, true);

                //    ControlUnitName.Dispatcher.BeginInvoke(
                //        new Action(() => ControlUnitName.Text = x.Success ? x.UnitName : "Unable to connect to controlunit"));
                //    ControlUnitCode.Dispatcher.BeginInvoke(
                //        new Action(
                //            () => ControlUnitCode.Text = x.Success ? x.ControlCode : "Unable to connect to controlunit"));
                //}

                decimal grossTotal = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossTotal), 2);

                var totalVat = orderDetails.Sum(ol => ol.VatAmount());

                decimal netTotal = grossTotal - totalVat;
                // printModel.GrandTotal = grossTotal;
                // printModel.NetTotal = netTotal;
                txtEndTotal.Dispatcher.BeginInvoke(new Action(() => txtEndTotal.Text = Math.Round(grossTotal) + ",00 Kr"));
                if (Tip > 0)
                {
                    lblTip.Dispatcher.BeginInvoke(new Action(() => lblTip.Visibility = Visibility.Visible));
                    lblNetPayable.Dispatcher.BeginInvoke(new Action(() => lblNetPayable.Visibility = Visibility.Visible));
                    txtTip.Dispatcher.BeginInvoke(new Action(() => txtTip.Visibility = Visibility.Visible));
                    txtTip.Dispatcher.BeginInvoke(new Action(() => txtTip.Text = Math.Round(Tip) + ",00 Kr"));
                    txtNetPayable.Dispatcher.BeginInvoke(new Action(() => txtNetPayable.Visibility = Visibility.Visible));
                    txtNetPayable.Dispatcher.BeginInvoke(new Action(() => txtNetPayable.Text = Math.Round(grossTotal+Tip) + ",00 Kr"));
                }


                //  printModel.Items = orderDetails.Where(itm => itm.IsValid).ToList();
                LBItems.Dispatcher.BeginInvoke(
                    new Action(() => LBItems.ItemsSource = orderDetails.Where(itm => itm.IsValid).ToList()));
            }
            catch (Exception ex)
            {

                LogWriter.LogWrite(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            DirectPrint directPrint = new DirectPrint();
            directPrint.PrintBill(OrderId);
            DialogResult = true;
            //  SendPrint();

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        private void SendPrint()
        {
            try
            {
                var dlg = new PrintDialog();

                // GM:TODO
               
                string printer = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource) flowDocument).DocumentPaginator;
                //int countItems = LBItems.Items.Count;
                //int countvat = LBVats.Items.Count;
                //var totalheight = 220 + 45 + 8 + (countItems * 18) + 8 + 20 + 20 + (countvat * 18) + 300;

                // paginator.PageSize = new Size(350, 550 + itemheight + pheight);
                paginator.PageSize = new Size(280, gdMain.ActualHeight + 100);

                if (string.IsNullOrEmpty(printer))
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(paginator, "Order Print");
                        Close();
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
                    dlg.PrintQueue = prntque;
                    // dlg.PrintTicket.CopyCount = 3; // number of copies
                    // dlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
                    dlg.PrintDocument(paginator, "Print Order");
                    Close();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(UI.Message_PrintingProblemTryLater, Defaults.AppProvider.AppTitle, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RenderGridSize()
        {
            gdMain.Measure(new Size(double.MaxValue, double.MaxValue));
            Size visualSize = gdMain.DesiredSize;
            gdMain.Arrange(new Rect(new Point(0, 0), visualSize));
            gdMain.UpdateLayout();
        }
    }
}