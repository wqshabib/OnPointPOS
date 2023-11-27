using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using POSSUM.Model;
using POSSUM.Handlers;
using POSSUM.Res;


namespace POSSUM.Views.PrintOrder
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintEmployeeLogWindow : Window
    {
        public PrintEmployeeLogWindow()
        {
            InitializeComponent();

            dtpFrom.Text = DateTime.Now.ToLongDateString();
            dtpTo.Text = DateTime.Now.ToLongDateString();
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
            InfoCanvas.DataContext = Defaults.CompanyInfo;
            txtDescription.Text = UI.Global_Employee + " Log on " + DateTime.Now.ToShortDateString();
            PrintReport();
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
        public void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();

                string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Log");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(350, gdMain.ActualHeight + 300);

                if (string.IsNullOrEmpty(printerName))
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(paginator, "Journal Print");
                        Close();
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
                    dlg.PrintDocument(paginator, "Journal");
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

        private void PrintReport()
        {
            try
            {
                DateTime dtFrom = Convert.ToDateTime(dtpFrom.Text).Date;
                DateTime dtTo = Convert.ToDateTime(dtpTo.Text + "  11:59:00 PM");
                var logs = new List<EmployeeLog>();
                var progressDialog = new ProgressWindow();
                var backgroundThread = new Thread(() =>
                {
                    logs = ReportGenerator.PrintEmployeeLog(dtFrom, dtTo);

                    progressDialog.Closed += (arg, ew) => { progressDialog = null; };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
                });
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();
                FlowDocumentReader.Visibility = Visibility.Visible;
                txtDescription.Text = UI.Global_Employee + " Log from " + dtFrom.ToShortDateString() + " To " +
                                      dtTo.ToShortDateString();

                LBLogs.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void RenderGridSize()
        {
            gdMain.Measure(new Size(double.MaxValue, double.MaxValue));
            Size visualSize = gdMain.DesiredSize;
            gdMain.Arrange(new Rect(new Point(0, 0), visualSize));
            gdMain.UpdateLayout();
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }
    }
}