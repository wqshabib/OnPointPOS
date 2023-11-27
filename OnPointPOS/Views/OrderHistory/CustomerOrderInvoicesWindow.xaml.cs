using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using POSSUM.Model;
using System.Threading;
using POSSUM.Presenters.OrderHistory;
using POSSUM.Data;
using POSSUM.Res;
using System.IO;
using POSSUM.Pdf;
using System.Runtime.InteropServices;
using System.Printing;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing;
using System.Collections;
using POSSUM.Handlers;
using POSSUM.Views.PrintOrder;

namespace POSSUM.View.OrderHistory
{
    public partial class CustomerOrderInvoicesWindow : Window, IOrderHistoryView
    {
        OrderHistoryPresenter presenter;
        

        public CustomerOrderInvoicesWindow()
        {
            try
            {
 InitializeComponent();
            presenter = new OrderHistoryPresenter(this);
            if (App.MainWindow.ShiftNo == 0)
                App.MainWindow.ShiftNo = 1;
            dtpFrom.Text = DateTime.Now.ToShortDateString();
            dtpTo.Text = DateTime.Now.ToShortDateString();
            dtpOrderFrom.Text = DateTime.Now.ToShortDateString();
            dtpOrderTo.Text = DateTime.Now.ToShortDateString();
          
            presenter.HandleCustomerInvoiceSearchClick();
            FillCombo();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
           
        }
        private void FillCombo()
        {
            var repo = new InvoiceHandler();
            var customers = repo.GetAllCustomers();
            cmbCustomer.ItemsSource = customers;
            if (cmbCustomer.Items.Count > 0)
                cmbCustomer.SelectedIndex = 0;
           

        }
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleCustomerInvoiceSearchClick();
        }
        private void btnViewPending_Click(object sender, RoutedEventArgs e)
        {
            SearchPending();
        }
        private void SearchPending()
        {
            try
            {
                var customer = cmbCustomer.SelectedItem as Customer;
                var dtFrom = Convert.ToDateTime(dtpOrderFrom.Text).Date;
                var dtTo = Convert.ToDateTime(dtpOrderTo.Text + "  11:59:00 PM");
                if (customer != null)
                    dgPendingOrders.ItemsSource = presenter.HandleCustomerPendingOrderSearchClick(customer.Id, dtFrom, dtTo);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, UI.Global_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnRowLoading(object sender, DataGridRowEventArgs e)
        {

            if (e.Row.Item != null)
            {
                var order = e.Row.Item as Order;
                if (order != null)
                    e.Row.Background = Utilities.GetColorBrushFromOrderStatus(order.Status);
            }
        }
        #region Remove Item from Order and Show Order Detail
        Order currentOrderMaster = OrderFactory.CreateEmtpy();
        #endregion

       

        private void txtOrderNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //decimal val = 0;
            //e.Handled = !decimal.TryParse(e.Text, out val);
        }




        DateTime IOrderHistoryView.GetStartdate()
        {
            return Convert.ToDateTime(dtpFrom.Text);
        }

        DateTime IOrderHistoryView.GetEnddate()
        {
            return Convert.ToDateTime(dtpTo.Text + "  11:59:00 PM");
        }

        string IOrderHistoryView.GetQueryText()
        {
            return txtOrderNumber.Text;
        }

        public void SetResult(List<Order> orders)
        {
            var _orders = orders.OrderByDescending(o => o.InvoiceDate).ToList();
            dataGrid1.ItemsSource = _orders;
        }

        public void SetTotalAmount(decimal total)
        {
            txtOrdersTotal.Text = total.ToString("C");
        }

        public void ShowError(string title, string message)
        {
            App.MainWindow.ShowError(title, message);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonReturn_Click(object sender, RoutedEventArgs e)
        {
            var order = (Order)(sender as Button).DataContext;
            if (order != null)
            {
                ReturnOrderWindow retrunOrderWindow = new ReturnOrderWindow(order);
                retrunOrderWindow.ShowDialog();
            }
        }

        private void ButtonPrintInvoice_Click(object sender, RoutedEventArgs e)
        {
            var order = (Order)(sender as Button).DataContext;
            if (order != null)
            {
                try
                {
                    MemoryStream ms = GenerateOcrPdf(order.Id);
                    //  myImage = System.Drawing.Image.FromStream(ms);
                    string fileName = Environment.CurrentDirectory + @"\temp.pdf";
                    //  string fileName = @"C:\NIMPOS\temp.pdf";
                    FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    ms.WriteTo(file);
                    file.Close();
                    ms.Close();

                    //System.Windows.Forms.PrintDialog pd = new System.Windows.Forms.PrintDialog();
                    //pd.PrinterSettings = new PrinterSettings();
                    //if (System.Windows.Forms.DialogResult.OK == pd.ShowDialog())
                    //{
                    //    // Send a printer-specific to the printer.
                    //    PdfPrinterHelper.SendFileToPrinter(fileName, pd.PrinterSettings.PrinterName);
                    //}
                    string printerName = GetPrinterName();
                    PdfPrinterHelper.SendFileToPrinter(fileName, printerName);

                    //Direct send pdf file to printer
                    //   File.Copy(fileName, printerName, true);


                    //PrintDocument objPrintDocument = null;

                    //objPrintDocument = new PrintDocument();

                    //objPrintDocument.PrinterSettings.PrinterName = printerName;
                    ////the temp text file created and with data

                    //objPrintDocument.DocumentName = fileName;
                    ////I guess don't need this because I've setted up the file name (it is reachable)
                    //objPrintDocument.PrintPage += ObjPrintDocument_PrintPage;
                    ////send the file to the printer (this works)
                    //objPrintDocument.Print();

                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    MessageBox.Show(ex.Message);
                    // throw;
                }



            }
        }
        private System.Drawing.Image myImage;
        private void ObjPrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(myImage, System.Drawing.Point.Empty);
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

        private void ButtonInvoicePdf_Click(object sender, RoutedEventArgs e)
        {
            var order = (Order)(sender as Button).DataContext;
            if (order != null)
            {
                PrintOrderReceiptWindow printWindow = new PrintOrderReceiptWindow();
                printWindow.PrintInvoice(order.Id, true);
                // retrunOrderWindow.ShowDialog();
            }
        }



        private MemoryStream GenerateOcrPdf(Guid invoiceId)
        {
            //var orderRepo = new OrderRepository(PosState.GetInstance().Context);
            //var customerInvoice = orderRepo.GetCustomerInvoice(invoiceId);

            var orderRepo = new InvoiceHandler();
            var customerInvoice = orderRepo.GetCustomerInvoice2(invoiceId);

            var invoice = customerInvoice.Invoice;
            var orderDetails = customerInvoice.OrderDetails;
            var customer = customerInvoice.Customer;
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

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                List<Order> selectedOrders = new List<Order>();
                if (dgPendingOrders.SelectedItems != null)
                {
                    foreach (var order in dgPendingOrders.SelectedItems)
                    {
                        var od = order as Order;
                        selectedOrders.Add(od);
                    }
                    if (selectedOrders.Count > 0)
                    {
                        string remanrks = "";
                        remanrks = Utilities.PromptInput("Referens", UI.Sales_EnterComment, remanrks);
                        var customer = cmbCustomer.SelectedItem as Customer;
                        OrderRepository orderRepo = new OrderRepository(PosState.GetInstance().Context);
                        var invoiceId = orderRepo.GenerateCustomerInvoice(selectedOrders, customer.Id, remanrks);
                        GenerateOcrPdf(invoiceId);
                        SearchPending();
                        TC1.SelectedIndex = 0;
                        presenter.HandleCustomerInvoiceSearchClick();

                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            var order = dgPendingOrders.SelectedItem as Order;
            if (order != null)
            {
                order.IsSelected = true;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var order = dgPendingOrders.SelectedItem as Order;
            if (order != null)
            {
                order.IsSelected = false;
            }
        }

        private void cmbCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbCustomer.IsInitialized)
                SearchPending();
        }

        public DateTime GetStartdateCCFailed()
        {
            throw new NotImplementedException();
        }

        public DateTime GetEnddateCCFailed()
        {
            throw new NotImplementedException();
        }

        public string GetQueryTextCCFailed()
        {
            throw new NotImplementedException();
        }

        public void SetTotalAmountCCFailed(decimal total)
        {
            throw new NotImplementedException();
        }

        public void SetResultCCFailed(List<Order> orders)
        {
            throw new NotImplementedException();
        }
    }

}
