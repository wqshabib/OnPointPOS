using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using POSSUM.Model;
using POSSUM.Pdf;
using POSSUM.Handlers;
using POSSUM.Res;
using MessageBox = System.Windows.MessageBox;
using PrintDialog = System.Windows.Controls.PrintDialog;
using POSSUM.Data;

namespace POSSUM.Views.PrintOrder
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintOrderReceiptWindow
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private decimal grandTotal;

        private bool IsReturnInvoice;

        private Guid OrderId = default(Guid);
        private Order orderMaster;

        public PrintOrderReceiptWindow()
        {
            InitializeComponent();

            // gdMain.DataContext = Defaults.CompanyInfo;
        }

        public PrintOrderReceiptWindow(Guid orderId, bool isCopy)
        {
            InitializeComponent();

            GetOrderById(orderId, isCopy);
            RenderGridSize();
            // this.Height = gdMain.ActualHeight + 50;
        }

        public string marchantReceipt { get; set; }
        public string customerReceipt { get; set; }

        private MemoryStream GenerateOcrPdf(Guid InvoiceId)
        {

            var orderRepo = new InvoiceHandler();
            var customerInvoice = orderRepo.GetCustomerInvoice2(InvoiceId);

            var invoice = customerInvoice.Invoice;
            var orderDetails = customerInvoice.OrderDetails;
            var customer = customerInvoice.Customer;
            //Dictionary<string, string> orderLines = new Dictionary<string, string>();
            //foreach (var orderItem in orderDetails)
            //{
            //    orderLines.Add(orderItem.ItemName, Text.FormatNumber(orderItem.UnitPrice, 2));
            //}

            decimal grossTotal =
                Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
            // decimal itemsDiscount = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ItemDiscount), 2);
            var totalVat = orderDetails.Sum(ol => ol.VatAmount());

            decimal netTotal = grossTotal - totalVat;

            decimal decTotal = grossTotal;
            decimal decVATPercent = totalVat; // orderDetails.FirstOrDefault().VAT; // Get first VAT available

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
            DateTime dtExpireDate = DateTime.Now.AddDays(21);
            // invoice.LastRetryDateTime.Date.AddDays(invoice.Interval);

            return Ocr.Create(invoice.InvoiceNumber, Defaults.Outlet.Name, invoice.CreationDate, dtExpireDate,
                customer.Name, strOcr, invoice.InvoiceNumber, invoice.Remarks
                // orderMaster.InvoiceNumber //  string.Concat(invoice.tOrder.tCustomer.tCompany.tCorporate.FirstName, " ", invoice.tOrder.tCustomer.tCompany.tCorporate.LastName)
                , " ", orderDetails, Defaults.Terminal.UniqueIdentification + "-" + invoice.InvoiceNumber,
                "Customer Invoice", decTotal, decVATPercent, netTotal, strReminderText,
                string.Concat(customer.Name, " ", customer.OrgNo), customer.Address1,
                string.Concat(customer.ZipCode, " ", customer.City), dtExpireDate, decReminderFee);

            // decTotalAmount * companyAgreement.DibsPartnerPercent / 100
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //  GetOrderById(OrderId, false);
            Dispatcher.BeginInvoke((Action)(() =>
           {
               var ms = GenerateOcrPdf(OrderId);

                //write to file

                SaveFileDialog saveFileDialog = new SaveFileDialog();
               saveFileDialog.FileName = "Faktura"; // Default file name
                saveFileDialog.DefaultExt = ".pdf"; // Default file extension
                saveFileDialog.Filter = "Text documents (.pdf)|*.pdf"; // Filter files by extension
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               {
                   string fileName = saveFileDialog.FileName; // + ".pdf";

                    FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                   ms.WriteTo(file);
                   file.Close();
                   ms.Close();
               }
                // SendToPrint(false);
            }));

            // run all background tasks here
        }

        public void GetOrderById(Guid orderId, bool isCopy)
        {
            try
            {
                var orderRepo = new OrderRepository(PosState.GetInstance().Context);
                orderMaster = orderRepo.GetOrderMaster(orderId);

                var receipt = orderRepo.GetOrderReceipt(orderId);

                var printModel = new PrintModel
                {
                    Footer = Defaults.Outlet.FooterText,
                    CompanyInfo = Defaults.CompanyInfo,
                    TaxDesc = Defaults.Outlet.TaxDescription,
                    AccountNumber = Defaults.CompanyInfo.BankAccountNo,
                    PaymentReceiver = Defaults.CompanyInfo.PaymentReceiver
                };

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
                if (orderMaster.CustomerId != default(Guid))
                {
                    var customer = new CustomerRepository().GetCustomerById(orderMaster.CustomerId);
                    printModel.Customer = string.IsNullOrEmpty(customer.Name) ? "Nill" : customer.Name;
                    txtCustomer.Dispatcher.BeginInvoke(new Action(() => txtCustomer.Text = customer.Name));
                }
                printModel.ReceiptNo = Defaults.Terminal.UniqueIdentification + "-" + receipt.ReceiptNumber;
                printModel.ReferenceNo = orderMaster.InvoiceNumber;
                txtFakturaRefNo.Dispatcher.BeginInvoke(
                    new Action(() => txtFakturaRefNo.Text = "#          " + orderMaster.InvoiceNumber + "  #"));

                printModel.ReceiptDate = DateTime.Now;
                printModel.FakturaDate = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
                printModel.Cashier = Defaults.User.UserName;
                txtCasheir.Dispatcher.BeginInvoke(new Action(() => txtCasheir.Text = Defaults.User.UserName));
                var orderDetails = orderRepo.GetOrderLinesById(orderId);

                printModel.Items = orderDetails;

                decimal grossTotal =
                    Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
                // decimal itemsDiscount = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ItemDiscount), 2);
                var totalVat = orderDetails.Sum(ol => ol.VatAmount());

                decimal netTotal = grossTotal - totalVat;
                printModel.GrandTotal = grossTotal; // - itemsDiscount;
                printModel.NetTotal = netTotal;
                printModel.VAT = totalVat;

                long intPart = (long)grossTotal;
                decimal fracPart = grossTotal - intPart;

                printModel.IntegerPart = (int)intPart;
                printModel.FractionalPart = fracPart;
                printModel.RoundedAmount = orderMaster.RoundedAmount;
                grandTotal = grossTotal;
                //  txtEndTotal.Dispatcher.BeginInvoke(new Action(() => txtEndTotal.Text =  grossTotal.ToString("C")));
                printModel.Items = orderDetails.Where(itm => itm.IsValid).ToList();
                LBItems.Dispatcher.BeginInvoke(
                    new Action(() => LBItems.ItemsSource = orderDetails.Where(itm => itm.IsValid).ToList()));

                Dispatcher.BeginInvoke(new Action(() => DataContext = printModel));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        public void PrintInvoice(Guid orderId, bool isCopy)
        {
            OrderId = orderId;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }

        public void PrintReturnInvoice(Guid orderId)
        {
            IsReturnInvoice = true;
            OrderId = orderId;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.JournalLog(
                Defaults.User.TrainingMode
                    ? Convert.ToInt16(JournalActionCode.ReceiptCopyPrintedViaTrainingMode)
                    : Convert.ToInt16(JournalActionCode.ReceiptCopyPrinted), orderMaster.Id);
            SendToPrint(true);
        }

        public void SendToPrint(bool isCopy)
        {
            try
            {
                var dlg = new PrintDialog();

                // GM:TODO
                // var ordRepo = new OrderRepository();
                // string printer = ordRepo.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                // paginator.PageSize = new Size(350, 550 + itemheight + pheight);
                paginator.PageSize = new Size(1000, gdMain.ActualHeight + 200);
                if (dlg.ShowDialog() == true)
                {
                    dlg.PrintDocument(paginator, "Print Order");
                }
                //if (string.IsNullOrEmpty(printer))
                //{
                //    PrintDialog printDialog = new PrintDialog();
                //    if (printDialog.ShowDialog() == true)
                //    {
                //        printDialog.PrintDocument(paginator, "Order Print");
                //        this.Close();
                //    }
                //}
                //else
                //{

                //    PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                //    PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                //    var data = printers.Where(pn => pn.Name == printer);
                //    if (data.Count() > 0)
                //    {
                //        prntque = printers.Single(pn => pn.Name == printer);
                //    }
                //    else // Fall back to XPS Document Writer
                //    {
                //        printer = "Microsoft XPS Document Writer";
                //        printers = new PrintServer().GetPrintQueues();
                //        prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                //        data = printers.Where(pn => pn.Name == printer);
                //        if (data.Count() > 0)
                //        {
                //            prntque = printers.Single(pn => pn.Name == printer);
                //        }
                //    }

                //    dlg.PrintQueue = prntque; //

                //    // dlg.PrintTicket.CopyCount = 3; // number of copies
                //    // dlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
                //    //if(isCopy==false)
                //    //    LogWriter.JournalLog(Defaults.User.TrainingMode ? Defaults.ActionList[JournalActionCode.ReceiptPrintedViaTrainingMode].Id : Defaults.ActionList[JournalActionCode.ReceiptPrinted].Id, orderMaster.Id);
                //    dlg.PrintDocument(paginator, "Print Order");
                //    this.Close();
                //}
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