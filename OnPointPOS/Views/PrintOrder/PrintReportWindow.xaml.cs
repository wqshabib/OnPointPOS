using System;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using POSSUM.Model;
using POSSUM.Handlers;
using POSSUM.Res;
using System.Text;

namespace POSSUM.Views.PrintOrder
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintReportWindow : Window
    {
        Guid reportId;
        public PrintReportWindow()
        {
            InitializeComponent();
        }
        public ReportType Type { get; set; }
        public string reportGlobleContent = "";
        public PrintReportWindow(string description, ReportType type, Guid reportId)
        {
            InitializeComponent();
            this.reportId = reportId;
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
            gdMain.DataContext = Defaults.CompanyInfo;
            txtDescription.Text = description;
            reportGlobleContent = description;
            lblFooter.Dispatcher.BeginInvoke(new Action(() => lblFooter.Text = Defaults.Outlet.FooterText));
            this.Type = type;
        }
        public void GenerateReport(string description, ReportType type)
        {
            try
            {
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
                gdMain.DataContext = Defaults.CompanyInfo;
                txtDescription.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtDescription.Text = description;
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        SendPrint();
                    }));
                }));
                lblFooter.Dispatcher.BeginInvoke(new Action(() => lblFooter.Text = Defaults.Outlet.FooterText));
                this.Type = type;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (reportId != default(Guid))
                {
                    int rptType = Type == ReportType.ZReport ? 1 : 0;
                    var reportContent = ReportGenerator.GetReport(reportId, rptType, Defaults.User.UserCode);

                    //var printWindow = new printReportWindow(reportContent, ReportType.ZReport);
                    //printWindow.ShowDialog();
                    Printing printing = new Printing();
                    printing.setPrinterName(Defaults.PrinterName);
                    
                    printing.print(reportContent);
                }
                else
                {
                    //SendPrint();
                    Printing printing = new Printing();
                    printing.setPrinterName(Defaults.PrinterName);
                    printing.print(reportGlobleContent);
                   
                }

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
                string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(300, gdMain.ActualHeight + 300);
                if (this.Type == ReportType.XReport)
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReportXPrinted));
                else if (this.Type == ReportType.ZReport)
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReportZPrinted));
                if (string.IsNullOrEmpty(printerName))
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(paginator, "X-Report Print");
                        this.Close();
                    }
                }
                else
                {

                    PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                    PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                    var data = printers.Where(pn => pn.Name == printerName);
                    if (data.Count() > 0)
                    {
                        prntque = printers.Single(pn => pn.Name == printerName);
                    }
                    else // Fall back to XPS Document Writer
                    {
                        printerName = "Microsoft XPS Document Writer";
                        printers = new PrintServer().GetPrintQueues();
                        prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                        data = printers.Where(pn => pn.Name == printerName);
                        if (data.Count() > 0)
                        {
                            prntque = printers.Single(pn => pn.Name == printerName);
                        }
                    }

                    dlg.PrintQueue = prntque;
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
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}